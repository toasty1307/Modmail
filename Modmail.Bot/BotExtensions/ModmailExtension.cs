using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
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
    
        public async Task<DiscordChannel> CreateThread(DiscordChannel logs, DiscordMember member, string openMessage,
        bool dm = true, DiscordMember mod = null!)
    {
        Log.Information("Creating thread for {Username} in {Name}", member.Username, logs.Guild.Name);
        var channel = await logs.Guild
            .CreateChannelAsync($"{member.Username}-{member.Discriminator}", ChannelType.Text, logs.Parent);
        await channel.CreateWebhookAsync("Modmail");
        var msg = await channel.SendMessageAsync(new DiscordEmbedBuilder
        {
            Title = $"{member.Username}#{member.Discriminator} `{member.Id}`",
            Description =
                $"{member.Mention} was created {Formatter.Timestamp(member.CreationTimestamp)}\nJoined {Formatter.Timestamp(member.JoinedAt)}\n{string.Join("", member.Roles.Select(Formatter.Mention))}",
            Color = new DiscordColor("2F3136")
        });
        await msg.PinAsync();
        try
        {
            await member.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Thread Created",
                Description = openMessage,
                Color = new DiscordColor("2F3136")
            });
        }
        catch (UnauthorizedException)
        {
            await channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Can't DM",
                Description = "The user has either blocked Modmail or have their DMs disabled",
                Color = new DiscordColor("2F3136")
            });
        }

        await logs.SendMessageAsync(new DiscordEmbedBuilder
        {
            Title = "Thread Created",
            Description = dm
                ? $"{Formatter.Mention(channel)} `#{channel.Name}` was created by {Formatter.Mention(member)} `{member.Username}#{member.Discriminator} ({member.Id})` by DM"
                : $"{Formatter.Mention(channel)} `#{channel.Name}` was created by {Formatter.Mention(mod)} `{mod.Username}#{mod.Discriminator} ({mod.Id})` to contact {Formatter.Mention(member)} `{member.Username}#{member.Discriminator} ({member.Id})`",
            Color = new DiscordColor("2F3136")
        });
        return channel;
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
