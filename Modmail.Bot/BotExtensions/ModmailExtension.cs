using DSharpPlus;
using DSharpPlus.Entities;
using Serilog;

namespace Modmail.Bot.BotExtensions;

public class ModmailExtension : BaseExtension
{
    protected override void Setup(DiscordClient client)
    {
        Client = client;
    }

    public async Task<DiscordChannel> CreateCategory(DiscordGuild guild, IEnumerable<DiscordOverwrite> overwrites)
    {
        Log.Information("Creating category channel in {Name}", guild.Name);
        var overwriteBuilders = new List<DiscordOverwriteBuilder>();
        foreach (var overwrite in overwrites)
        {
            var o = await new DiscordOverwriteBuilder(guild.EveryoneRole)
                .FromAsync(overwrite);
            overwriteBuilders.Add(o);
        }
        
        var channel = await guild.CreateChannelAsync("Modmail", ChannelType.Category, overwrites: overwriteBuilders);
        return channel;
    }

    public async Task<DiscordChannel> CreateLog(DiscordChannel categoryChannel)
    {
        Log.Information("Creating log channel in {Name}", categoryChannel.Guild.Name);
        var logs = await categoryChannel.Guild.CreateChannelAsync("Logs", ChannelType.Text, categoryChannel);
        await SendLogEmbed(logs);
        return logs;
    }

    public async Task SendLogEmbed(DiscordChannel logs)
    {
        var embed = new DiscordEmbedBuilder
        {
            Title = "Logs Channel",
            Description =
                "This is not the channel for all the modmail logs, you can change this using the `/config logchannel` command.",
            Color = new DiscordColor(0x2F3136)
        };
        await logs.SendMessageAsync(embed);
    }
}
