using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Modmail.Bot.Extensions;
using Modmail.Config;
using Modmail.Data;
using Modmail.Data.Entities;
using Serilog;

namespace Modmail.Bot.Commands;

public class ModmailCommands : ApplicationCommandModule
{
    // set by DI
    public GuildContext Database { get; set; } = null!;
    public BotConfig Config { get; set; } = null!;

    [SlashCommand("setup", "setup the server")]
    [SlashRequireBotPermissions(Permissions.ManageChannels | Permissions.ManageRoles)]
    [SlashRequireUserPermissions(Permissions.Administrator)]
    [SlashRequireGuild]
    public async Task SetupCommand(InteractionContext context)
    {
        Log.Information("Setting up modmail for {Guild}", context.Guild.Name);
        await context.CreateResponseAsync("Setting up modmail...");
        var guildEntity = await Database.FindAsync<GuildEntity>(context.Guild.Id);
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
        var icon = Database.Find<FileEntity>(context.Guild.IconUrl ?? "https://cdn.discordapp.com/embed/avatars/0.png");
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
            Database.Add(icon);
        Database.Add(configEntity);
        Database.Add(guildEntity);
        await context.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Setup complete!"));
        await Database.SaveChangesAsync();
        Log.Information("Setup complete for {Guild}", context.Guild.Name);
    }
}