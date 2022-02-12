using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modmail.Bot.Extensions;
using Modmail.Config;
using Modmail.Data;
using Modmail.Data.Entities;
using Serilog;

namespace Modmail.Bot.Commands;

public class ModmailCommands : ApplicationCommandModule
{
    // set by DI
    public BotConfig Config { get; set; } = null!;
    private readonly ILogger<ModmailCommands> _logger;

    public ModmailCommands()
    {
        _logger = new Logger<ModmailCommands>(new LoggerFactory().AddSerilog());
    }

    [SlashCommand("setup", "setup the server")]
    [SlashRequireBotPermissions(Permissions.ManageChannels | Permissions.ManageRoles)]
    [SlashRequireUserPermissions(Permissions.Administrator)]
    [SlashRequireGuild]
    public async Task SetupCommand(InteractionContext context)
    {
        await using var db = new GuildContext();
        _logger.LogInformation("Setting up modmail for {Guild}", context.Guild.Name);
        await context.CreateResponseAsync("Setting up modmail...");
        var guildEntity = await db.FindAsync<GuildEntity>(context.Guild.Id);
        if (guildEntity is not null)
        {
            await context.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Already setup!"));
            return;
        }

        var ext = context.Client.GetModmailExtension();
        var categoryChannel = await ext.CreateCategory(context.Guild, context.Channel.PermissionOverwrites);
        var logsChannel = await ext.CreateLog(categoryChannel);
        var role = await context.Guild.CreateRoleAsync("Modmail log access");
        var downloadIcon = async () =>
        {
            var client = new HttpClient();
            var s = await client.GetStreamAsync(
                context.Guild.IconUrl ?? "https://cdn.discordapp.com/embed/avatars/0.png");
            var ms = new MemoryStream();
            await s.CopyToAsync(ms);
            return ms.ToArray();
        };
        var icon = db.Find<FileEntity>(context.Guild.IconUrl ?? "https://cdn.discordapp.com/embed/avatars/0.png");
        var iconExists = icon is not null;
        icon ??= new FileEntity
        {
            Url = context.Guild.IconUrl ?? "https://cdn.discordapp.com/embed/avatars/0.png",
            Data = await downloadIcon()
        };
        var configEntity = new ConfigEntity
        {
            CategoryChannelId = categoryChannel.Id,
            GuildId = context.Guild.Id,
            LogChannelId = logsChannel.Id,
            LogAccessRoleId = role.Id,
            MoveChannelsToCategory = true
        };
        guildEntity = new GuildEntity
        {
            Name = context.Guild.Name,
            Id = context.Guild.Id,
            Setup = true,
            Config = configEntity,
            Icon = icon
        };
        configEntity.GuildEntity = guildEntity;
        if (!iconExists)
            db.Add(icon);
        db.Add(configEntity);
        db.Add(guildEntity);
        await context.EditResponseAsync("Setup complete!");
        await db.SaveChangesAsync();
        _logger.LogInformation("Setup complete for {Guild}", context.Guild.Name);
    }

    [SlashCommand("contact", "contact a user", false)]
    [SlashRequirePermissions(Permissions.ManageChannels)]
    [SlashRequireGuild]
    public async Task ContactCommand(InteractionContext ctx,
        [Option("user", "the user to contact")] DiscordUser user)
    {
        await ctx.DeferAsync();

        if (user.IsBot)
        {
            var res = await ctx.EditResponseAsync("r/therewasanattempt");
            await res.Error();
            return;
        }

        var member = await ctx.Guild.GetMemberAsync(user.Id);
        if (member is null)
        {
            var res = await ctx.EditResponseAsync("User not in the guild");
            await res.Error();
            return;
        }
        
        await using var db = new GuildContext();
        var guildEntity = db.Guilds
            .Include(x => x.Threads)
            .Include(x => x.Config)
            .FirstOrDefault(x => x.Id == ctx.Guild.Id);
        
        if (guildEntity is null || !guildEntity.Setup)
        {
            var res = await ctx.EditResponseAsync("This server is not setup!");
            await res.Error();
            return;
        }
        
        if (guildEntity.Threads.Any(x => x.RecipientId == user.Id && x.Open))
        {
            var res = await ctx.EditResponseAsync("This user already has a thread!");
            await res.Error();
            return;
        }
        
        var ext = ctx.Client.GetModmailExtension();
        var logs = ctx.Guild.GetChannel(guildEntity.Config.LogChannelId);
        var thread = await ext.CreateThread(logs, member, guildEntity.Config.ModThreadOpenMessage!, false, ctx.Member);
        
        var avatar = await db.FindAsync<FileEntity>(member.AvatarUrl);
        var newAddition = avatar is null;
        avatar ??= new FileEntity
        {
            Url = member.AvatarUrl,
            Data = await new Uri(member.AvatarUrl).GetBytes()
        };
        var threadEntity = new ThreadEntity
        {
            Open = true,
            Created = DateTime.UtcNow,
            GuildId = ctx.Guild.Id,
            Id = thread.Id,
            GuildEntity = guildEntity,
            RecipientId = user.Id,
            RecipientName = $"{member.Username}#{member.Discriminator}",
            RecipientAvatar = avatar
        };
        
        guildEntity.Threads.Add(threadEntity);
        db.Add(threadEntity);
        if (newAddition)
            db.Add(avatar);
        db.Update(guildEntity);
        await db.SaveChangesAsync();
        var res1 = await ctx.EditResponseAsync($"{Formatter.Mention(thread)} `#{thread.Name} ({thread.Id})`");
        await res1.Success();
    }
    
    [ContextMenu(ApplicationCommandType.UserContextMenu, "Contact"/*, false*/)] // TODO
    [SlashRequireBotPermissions(Permissions.ManageChannels)]
    [SlashRequireGuild]
    public async Task ContactContext(ContextMenuContext ctx)
    {
        await ctx.DeferAsync();

        var member = ctx.TargetMember;
        if (member.IsBot)
        {
            var res = await ctx.EditResponseAsync("r/therewasanattempt");
            await res.Error();
            return;
        }
        
        await using var db = new GuildContext();
        var guildEntity = db.Guilds
            .Include(x => x.Threads)
            .Include(x => x.Config)
            .FirstOrDefault(x => x.Id == ctx.Guild.Id);
        
        if (guildEntity is null || !guildEntity.Setup)
        {
            var res = await ctx.EditResponseAsync("This server is not setup!");
            await res.Error();
            return;
        }
        
        if (guildEntity.Threads.Any(x => x.RecipientId == member.Id && x.Open))
        {
            var res = await ctx.EditResponseAsync("This user already has a thread!");
            await res.Error();
            return;
        }
        
        var ext = ctx.Client.GetModmailExtension();
        var logs = ctx.Guild.GetChannel(guildEntity.Config.LogChannelId);
        var thread = await ext.CreateThread(logs, member, guildEntity.Config.ModThreadOpenMessage!, false, ctx.Member);
        
        var avatar = await db.FindAsync<FileEntity>(member.AvatarUrl);
        var newAddition = avatar is null;
        avatar ??= new FileEntity
        {
            Url = member.AvatarUrl,
            Data = await new Uri(member.AvatarUrl).GetBytes()
        };
        var threadEntity = new ThreadEntity
        {
            Open = true,
            Created = DateTime.UtcNow,
            GuildId = ctx.Guild.Id,
            Id = thread.Id,
            GuildEntity = guildEntity,
            RecipientId = member.Id,
            RecipientName = $"{member.Username}#{member.Discriminator}",
            RecipientAvatar = avatar
        };
        
        guildEntity.Threads.Add(threadEntity);
        db.Add(threadEntity);
        if (newAddition)
            db.Add(avatar);
        db.Update(guildEntity);
        await db.SaveChangesAsync();
        var res1 = await ctx.EditResponseAsync($"{Formatter.Mention(thread)} `#{thread.Name} ({thread.Id})`");
        await res1.Success();
    }
    
    [SlashCommand("selfcontact", "contact yourself", false)]
    [SlashRequireGuild]
    public Task SelfContactCommand(InteractionContext ctx)
    {
        return ContactCommand(ctx, ctx.Member);
    }
}