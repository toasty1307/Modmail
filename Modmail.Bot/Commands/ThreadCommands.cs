using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Microsoft.Extensions.Logging;
using Modmail.Bot.Extensions;
using Serilog;

namespace Modmail.Bot.Commands;

// TODO only allow in threads when v2 releases
public class ThreadCommands : ApplicationCommandModule
{
    private ILogger<ThreadCommands> _logger;

    public ThreadCommands()
    {
        _logger = new Logger<ThreadCommands>(new LoggerFactory().AddSerilog());
    }

    [SlashCommand("reply", "replies to a thread")]
    [SlashRequireGuild]
    public async Task Reply(InteractionContext ctx,
        [Option("message", "the message to send to the user")] string message/*, 
        [Option("attachment1", "the attachment to send"     )] DiscordAttachment attachment1  = null!, 
        [Option("attachment2", "another attachment to send" )] DiscordAttachment attachment2  = null!,
        [Option("attachment2", "another attachment to send" )] DiscordAttachment attachment3  = null!,
        [Option("attachment2", "another attachment to send" )] DiscordAttachment attachment4  = null!, 
        [Option("attachment2", "another attachment to send" )] DiscordAttachment attachment5  = null!,
        [Option("attachment2", "another attachment to send" )] DiscordAttachment attachment6  = null!,
        [Option("attachment2", "another attachment to send" )] DiscordAttachment attachment7  = null!, 
        [Option("attachment2", "another attachment to send" )] DiscordAttachment attachment8  = null!,
        [Option("attachment2", "another attachment to send" )] DiscordAttachment attachment9  = null!,
        [Option("attachment2", "another attachment to send" )] DiscordAttachment attachment10 = null!*/
        )
    {
        await ctx.DeferAsync(true);
        var ext = ctx.Client.GetModmailExtension();
        await ext.SendWebhookMessage(ctx.Channel, message, new DiscordAttachment[]
        {
            /*
            attachment1, attachment2, attachment3, attachment4, attachment5,
            attachment6, attachment7, attachment8, attachment9, attachment10,
        */
        }, false, ctx.Member.AvatarUrl, $"{ctx.Member.Username}#{ctx.Member.Discriminator} (Reply)");
        var res = await ctx.EditResponseAsync("Sent!");
    }
    
    [SlashCommand("areply", "replies to a thread anonymously")]
    [SlashRequireGuild]
    public async Task AReply(InteractionContext ctx,
        [Option("message", "the message to send to the user")] string message/*, 
        [Option("attachment1", "the attachment to send"     )] DiscordAttachment attachment1  = null!, 
        [Option("attachment2", "another attachment to send" )] DiscordAttachment attachment2  = null!,
        [Option("attachment2", "another attachment to send" )] DiscordAttachment attachment3  = null!,
        [Option("attachment2", "another attachment to send" )] DiscordAttachment attachment4  = null!, 
        [Option("attachment2", "another attachment to send" )] DiscordAttachment attachment5  = null!,
        [Option("attachment2", "another attachment to send" )] DiscordAttachment attachment6  = null!,
        [Option("attachment2", "another attachment to send" )] DiscordAttachment attachment7  = null!, 
        [Option("attachment2", "another attachment to send" )] DiscordAttachment attachment8  = null!,
        [Option("attachment2", "another attachment to send" )] DiscordAttachment attachment9  = null!,
        [Option("attachment2", "another attachment to send" )] DiscordAttachment attachment10 = null!*/
    )
    {
        await ctx.DeferAsync(true);
        var ext = ctx.Client.GetModmailExtension();
        await ext.SendWebhookMessage(ctx.Channel, message, new DiscordAttachment[]
        {
            /*
            attachment1, attachment2, attachment3, attachment4, attachment5,
            attachment6, attachment7, attachment8, attachment9, attachment10,
        */
        }, true);
        var res = await ctx.EditResponseAsync("Sent!");
    }
}