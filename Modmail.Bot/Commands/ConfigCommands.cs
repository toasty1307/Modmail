using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Modmail.Bot.Extensions;
using Modmail.Config;
using Modmail.Data;
using Modmail.Data.Entities;

namespace Modmail.Bot.Commands;

[SlashCommandGroup("config", "config commands")]
public class ConfigCommands : ApplicationCommandModule
{
    // set by DI
    public GuildContext Database { get; set; } = null!;
    public BotConfig Config { get; set; } = null!;

    public override async Task<bool> BeforeSlashExecutionAsync(InteractionContext ctx)
    {
        var guildEntity = await Database.FindAsync<ConfigEntity>(ctx.Guild.Id);
        if (guildEntity != null) return true;
        await ctx.CreateResponseAsync("This server has not been configured yet. Please use the `setup` command to configure this server.");
        return false;
    }

    [SlashCommand("logchannel", "gets or sets the log channel for this server")]
    public async Task LogChannel(InteractionContext context, 
        [Option("channel", "the new log channel")] DiscordChannel channel = null!)
    {
        await context.DeferAsync();
        var config = await Database.FindAsync<ConfigEntity>(context.Guild.Id) ?? throw new InvalidOperationException("Config not found"); // should never throw
        if (channel == null!)
        {
            await context.FollowUpAsync($"The log channel is currently set to <#{config.LogChannelId}>");
            return;
        }

        if (!context.Member.Permissions.HasFlag(Permissions.Administrator))
        {
            await context.FollowUpAsync("You must be an administrator to set the log channel");
            return;
        }

        config.LogChannelId = channel.Id;
        await context.FollowUpAsync($"Changed the logs channel to <#{config.LogChannelId}>");
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
            await context.FollowUpAsync($"The category channel is currently set to <#{config.CategoryChannelId}>");
            return;
        }

        if (!context.Member.Permissions.HasFlag(Permissions.Administrator))
        {
            await context.FollowUpAsync("You must be an administrator to set the category channel");
            return;
        }

        config.CategoryChannelId = channel.Id; 
        await context.FollowUpAsync($"Changed the category channel to <#{config.CategoryChannelId}>");
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
            await context.FollowUpAsync($"The log access role is currently set to <\\@&{config.LogAccessRoleId}>"); // TODO fix the mention thing
            return;
        }

        if (!context.Member.Permissions.HasFlag(Permissions.Administrator))
        {
            await context.FollowUpAsync("You must be an administrator to set the log access role");
            return;
        }

        config.LogAccessRoleId = role.Id;
        await context.FollowUpAsync($"Changed the log access role to <\\@&{config.LogAccessRoleId}>");
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
            await context.FollowUpAsync("You must be an administrator to set the log access role");
            return;
        }

        config.MoveChannelsToCategory = move;
        await context.FollowUpAsync(
            move 
            ? "oki i will move the channels to the category uwu" 
            : "oki i wont move the channels to the category channel owo");
        Database.Update(config);
        await Database.SaveChangesAsync();
    }
}