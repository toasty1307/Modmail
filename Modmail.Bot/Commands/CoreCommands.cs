using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Modmail.Config;

namespace Modmail.Bot.Commands;

public class CoreCommands : ApplicationCommandModule
{
    // set by DI
    public BotConfig Config { get; set; } = null!;

    [SlashCommand("ping", "Shows the bot's latency")]
    public async Task PingCommand(InteractionContext context)
    {
        await context.CreateResponseAsync($"Pong! The latency is {context.Client.Ping}ms");
    }
    
    [SlashCommand("invite", "invite")]
    public async Task InviteCommand(InteractionContext context)
    {
        await context.CreateResponseAsync(new DiscordInteractionResponseBuilder()
            .WithContent("Click the button below to invite me to your server!")
            .AddComponents(new DiscordLinkButtonComponent(Config.InviteLink, "Invite me!")));
    }
}