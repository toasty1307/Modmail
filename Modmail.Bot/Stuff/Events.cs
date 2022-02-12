using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.EventArgs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modmail.Bot.Extensions;
using Modmail.Data;

namespace Modmail.Bot.Stuff;

public class Events
{
    private readonly ILogger<Events> _logger;

    public Events(ILogger<Events> logger)
    {
        _logger = logger;
    }

    public async Task OnGuildDownloadCompleted(DiscordClient sender, GuildDownloadCompletedEventArgs e)
    {
        await using var db = new GuildContext();
        foreach (var (id, guild) in e.Guilds)
        {
            var entity = db.Guilds
                .Include(x => x.Config)
                .Include(x => x.Threads)
                .FirstOrDefault(x => x.Id == id);
            if (entity is null || !entity.Setup)
                continue;

            var logs = guild.GetChannel(entity.Config.LogChannelId);
            var cat = guild.GetChannel(entity.Config.CategoryChannelId);
            var ext = sender.GetModmailExtension();
            if (logs is null && cat is not null)
            {
                logs = await ext.CreateLog(cat);
                entity.Config.LogChannelId = logs.Id;
                db.Update(entity);
                db.Update(entity.Config);
                await db.SaveChangesAsync();
            }
            else if (logs is not null && cat is null)
            {
                cat = await ext.CreateCategory(guild, logs.PermissionOverwrites);
                entity.Config.CategoryChannelId = cat.Id; 
                await logs.ModifyPositionAsync(0, parentId: cat.Id);
                db.Update(entity);
                db.Update(entity.Config);
                await db.SaveChangesAsync();
            }
            else if (logs is null && cat is null)
            {
                var threads = entity.Threads
                    .Where(x => x.Open)
                    .Select(x => (x, guild.GetChannel(x.Id))).ToList();
                
                var thread = threads.FirstOrDefault(x => x.Item2 is not null);
                if (thread is (null, null))
                {
                    entity.Setup = false;
                    entity.Threads.ForEach(x => x.Open = false);
                    foreach (var entityThread in entity.Threads)
                    {
                        await ext.ThreadDeleted(null!, entityThread, "Deleted when the bot was offline.", guild.Name);
                    }
                    db.Update(entity);
                    await db.SaveChangesAsync();
                    _logger.LogInformation("{Name} got wiped completely", guild.Name);
                    return;
                }

                cat = await ext.CreateCategory(guild, thread.Item2!.PermissionOverwrites);
                logs = await ext.CreateLog(cat);

                foreach (var discordChannel in threads)
                {
                    if (thread.Item2 is null)
                    {
                        discordChannel.x.Open = false;
                        await ext.ThreadDeleted(logs, discordChannel.x, "Deleted when the bot was offline.");
                    }
                    else
                        await discordChannel.Item2.ModifyPositionAsync(0, parentId: cat.Id);
                }
                
                entity.Config.LogChannelId = logs.Id;
                entity.Config.CategoryChannelId = cat.Id;
                db.Update(entity);
                db.Update(entity.Config);
                await db.SaveChangesAsync();
            }

            foreach (var entityThread in entity.Threads.Where(x => x.Open))
            {
                var channel = guild.GetChannel(entityThread.Id);
                if (channel is not null)
                    await channel.ModifyPositionAsync(0, parentId: cat!.Id);
                else
                {
                    await ext.ThreadDeleted(logs!, entityThread, "Deleted when the bot was offline.");
                    entityThread.Open = false;
                    db.Update(entityThread);
                    await db.SaveChangesAsync();
                }
            }
        }
    }

    public async Task SlashCommandErrored(SlashCommandsExtension sender, SlashCommandErrorEventArgs e)
    {
        await OnInteractionErrored(e.Context.Interaction, e.Exception, e.Context.CommandName);
    }

    public async Task ContextMenuErrored(SlashCommandsExtension sender, ContextMenuErrorEventArgs e)
    {
        await OnInteractionErrored(e.Context.Interaction, e.Exception, e.Context.CommandName);
    }
    
    private async Task OnInteractionErrored(DiscordInteraction interaction, Exception exception, string commandName)
    {
        var reply = exception switch
        {
            SlashExecutionChecksFailedException checksException => checksException.FailedChecks[0] switch
            {
                SlashRequireBotPermissionsAttribute => "Missing Bot permissions",
                SlashRequireDirectMessageAttribute => "DM only",
                SlashRequireGuildAttribute => "Guild only",
                SlashRequireOwnerAttribute => "Owner only",
                SlashRequirePermissionsAttribute => "Missing Bot or User permissions",
                SlashRequireUserPermissionsAttribute => "Missing User Permissions",
                _ => throw new ArgumentOutOfRangeException(),
            },
            _ => "Unknown Error",
        };
        try
        {
            _logger.LogError(exception, "Error in {Command}", commandName);
            var original = await interaction.GetOriginalResponseAsync();
            if (original is null)
            {
                await interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral(true).WithContent(reply));
                original = await interaction.GetOriginalResponseAsync();
                await original.Error();
            }
            else
            {
                await interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent(reply));
                await original.Error();
            }
        }
        catch
        {
            _logger.LogError(exception, "Error in {Command}", commandName);
            throw; // debugger stop
        }
    }

    public async Task OnChannelDeleted(DiscordClient sender, ChannelDeleteEventArgs e)
    {
        await using var context = new GuildContext();
        var guildEntity = context.Guilds
            .Include(x => x.Config)
            .Include(x => x.Threads)
            .FirstOrDefault(x => x.Id == e.Channel.GuildId);

        if (guildEntity is null || !guildEntity.Setup)
            return;

        var ext = sender.GetModmailExtension();
        
        if (e.Channel.Id == guildEntity.Config.LogChannelId)
        {
            _logger.LogInformation("Log channel deleted in {Guild}, creating a new one cuz you cant break me", e.Channel.Guild.Name);
            var logs = await ext.CreateLog(e.Guild.GetChannel(guildEntity.Config.CategoryChannelId));
            guildEntity.Config.LogChannelId = logs.Id;
            context.Update(guildEntity);
            context.Update(guildEntity.Config);
            await context.SaveChangesAsync();
            return;
        }
        
        if (e.Channel.Id == guildEntity.Config.CategoryChannelId)
        {
            _logger.LogInformation("Category channel deleted in {Guild}, creating a new one cuz you cant break me", e.Channel.Guild.Name);
            var logs = e.Guild.GetChannel(guildEntity.Config.LogChannelId);
            var cat = await ext.CreateCategory(e.Guild, logs.PermissionOverwrites);
            guildEntity.Config.CategoryChannelId = cat.Id;
            var threads = guildEntity.Threads.Where(x => x.Open).Select(x => e.Guild.GetChannel(x.Id)).ToList();
            foreach (var discordChannel in threads)
            {
                await discordChannel.ModifyPositionAsync(0, parentId: cat.Id);
            }
            await logs.ModifyPositionAsync(0, parentId: guildEntity.Config.CategoryChannelId);
            await logs.SendMessageAsync("r/therewasanattempt");
            context.Update(guildEntity);
            context.Update(guildEntity.Config);
            await context.SaveChangesAsync();
            return;
        }

        var thread = guildEntity.Threads.FirstOrDefault(x => x.Id == e.Channel.Id);
        if (thread is not null)
        {
            _logger.LogInformation("Thead {Thread} deleted in {Guild}", thread.RecipientName, e.Channel.Guild.Name);
            var auditLogs = await e.Channel.Guild.GetAuditLogsAsync(1, action_type: AuditLogActionType.ChannelDelete);
            var audit = auditLogs.FirstOrDefault();
            var reason = audit is null 
                ? "_Couldn't get audit logs_"
                : $"{Formatter.Mention(audit.UserResponsible)} (`{audit.UserResponsible.Username}#{audit.UserResponsible.Discriminator} {audit.UserResponsible.Id}`) deleted the thread";
            await ext.ThreadDeleted(e.Guild.GetChannel(guildEntity.Config.LogChannelId), thread, reason);
            thread.Open = false;
            context.Update(thread);
            await context.SaveChangesAsync();
        }
    }
}