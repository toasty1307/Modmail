using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Modmail.Bot.Extensions;
using Modmail.Config;
using Modmail.Data;
using Modmail.Data.Entities;

namespace Modmail.Bot.Commands;

[SlashCommandGroup("config", "config commands"/*, false*/)] // TODO
public class ConfigCommands : ApplicationCommandModule
{
    // set by DI
    public GuildContext Database { get; set; } = null!;
    public BotConfig Config { get; set; } = null!;
    
    [SlashCommand("logchannel", "gets or sets the log channel for this server")]
    public async Task LogChannel(InteractionContext context, 
        [Option("channel", "the new log channel")] DiscordChannel channel = null!)
    {
        await context.DeferAsync();
        var config = await Database.FindAsync<ConfigEntity>(context.Guild.Id) ?? throw new InvalidOperationException("Config not found"); // should never throw
        if (channel == null!)
        {
            await context.EditResponseAsync($"The log channel is currently set to <#{config.LogChannelId}>");
            return;
        }

        if (!context.Member.Permissions.HasFlag(Permissions.Administrator))
        {
            await context.EditResponseAsync("You must be an administrator to set the log channel");
            return;
        }

        config.LogChannelId = channel.Id;
        await context.EditResponseAsync($"Changed the logs channel to <#{config.LogChannelId}>");
        if (config.MoveChannelsToCategory)
            await channel.ModifyPositionAsync(0, parentId: config.CategoryChannelId);
        var ext = context.Client.GetModmailExtension();
        await ext.SendLogEmbed(channel);
        Database.Update(config);
        await Database.SaveChangesAsync();
    }
    
    [SlashCommand("categorychannel", "gets or sets the category channel for this server")]
    public async Task CategoryChannel(InteractionContext context, 
        [Option("channel", "the new category channel")] DiscordChannel channel = null!)
    {
        await context.DeferAsync();
        var config = await Database.FindAsync<ConfigEntity>(context.Guild.Id) ?? throw new InvalidOperationException("Config not found"); // should never throw
        if (channel == null!)
        {
            await context.EditResponseAsync($"The category channel is currently set to <#{config.CategoryChannelId}>");
            return;
        }

        if (!context.Member.Permissions.HasFlag(Permissions.Administrator))
        {
            await context.EditResponseAsync("You must be an administrator to set the category channel");
            return;
        }

        config.CategoryChannelId = channel.Id; 
        await context.EditResponseAsync($"Changed the category channel to <#{config.CategoryChannelId}>");
        Database.Update(config);
        await Database.SaveChangesAsync();
    }
    
    [SlashCommand("logrole", "gets or sets the log viewer role for this server")]
    public async Task LogRole(InteractionContext context, 
        [Option("role", "the new log viewer role")] DiscordRole role = null!)
    {
        await context.DeferAsync();
        var config = await Database.FindAsync<ConfigEntity>(context.Guild.Id) ?? throw new InvalidOperationException("Config not found"); // should never throw
        if (role == null!)
        {
            await context.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent($"The log access role is currently set to <@&{config.LogAccessRoleId}>")
                .AddMentions(Array.Empty<IMention>()));
            return;
        }

        if (!context.Member.Permissions.HasFlag(Permissions.Administrator))
        {
            await context.EditResponseAsync("You must be an administrator to set the log access role");
            return;
        }

        config.LogAccessRoleId = role.Id;
        await context.EditResponseAsync(new DiscordWebhookBuilder()
            .WithContent($"Changed the log access role to <@&{config.LogAccessRoleId}>")
            .AddMentions(Array.Empty<IMention>()));
        Database.Update(config);
        await Database.SaveChangesAsync();
    }
    
    [SlashCommand("modopenmessage", "gets or sets the modopenmessage for this server")]
    public async Task ModOpenMessage(InteractionContext context, 
        [Option("message", "the new mod thread open message")] string message = null!)
    {
        await context.DeferAsync();
        var config = await Database.FindAsync<ConfigEntity>(context.Guild.Id) ?? throw new InvalidOperationException("Config not found"); // should never throw
        if (message == null!)
        {
            await context.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent($"The thread open message is currently set to `{config.ModThreadOpenMessage}`")
                .AddMentions(Array.Empty<IMention>()));
            return;
        }

        if (!context.Member.Permissions.HasFlag(Permissions.Administrator))
        {
            await context.EditResponseAsync("You must be an administrator to set the log access role");
            return;
        }

        config.ModThreadOpenMessage = message;
        await context.EditResponseAsync(new DiscordWebhookBuilder()
            .WithContent($"Changed the mod open thread message to `{message}`")
            .AddMentions(Array.Empty<IMention>()));
        Database.Update(config);
        await Database.SaveChangesAsync();
    }
    
    [SlashCommand("useropenmessage", "gets or sets the useropenmessage for this server")]
    public async Task UserOpenMessage(InteractionContext context, 
        [Option("message", "the new user thread open message")] string message = null!)
    {
        await context.DeferAsync();
        var config = await Database.FindAsync<ConfigEntity>(context.Guild.Id) ?? throw new InvalidOperationException("Config not found"); // should never throw
        if (message == null!)
        {
            await context.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent($"The thread open message is currently set to `{config.UserThreadOpenMessage}`")
                .AddMentions(Array.Empty<IMention>()));
            return;
        }

        if (!context.Member.Permissions.HasFlag(Permissions.Administrator))
        {
            await context.EditResponseAsync("You must be an administrator to set the log access role");
            return;
        }

        config.UserThreadOpenMessage = message;
        await context.EditResponseAsync(new DiscordWebhookBuilder()
            .WithContent($"Changed the mod open thread message to `{message}`")
            .AddMentions(Array.Empty<IMention>()));
        Database.Update(config);
        await Database.SaveChangesAsync();
    }

    [SlashCommand("movechannels", "gets or sets if the channels should move to the category channel")]
    public async Task MoveChannels(InteractionContext context,
        [Option("value", "if the channels should move to the category channel or not")]
        bool move = false)
    {
        await context.DeferAsync();
        var config = await Database.FindAsync<ConfigEntity>(context.Guild.Id) ??
                     throw new InvalidOperationException("Config not found"); // should never throw

        if (!context.Member.Permissions.HasFlag(Permissions.Administrator))
        {
            await context.EditResponseAsync("You must be an administrator to set the log access role");
            return;
        }

        config.MoveChannelsToCategory = move;
        await context.EditResponseAsync(
            move 
            ? "oki i will move the channels to the category uwu" 
            : "oki i wont move the channels to the category channel owo");
        Database.Update(config);
        await Database.SaveChangesAsync();
    }
}