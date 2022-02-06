using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace Modmail.Bot.Extensions;

public static class ContextExtensions
{
    public static async Task FollowUpAsync(this InteractionContext context, string message)
    {
        await context.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent(message));
    }
    
    public static async Task FollowUpAsync(this ContextMenuContext context, string message)
    {
        await context.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent(message));
    } 
    
    public static async Task FollowUpAsync(this InteractionContext context, DiscordEmbed embed)
    {
        await context.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed));
    }
    
    public static async Task FollowUpAsync(this ContextMenuContext context, DiscordEmbed embed)
    {
        await context.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed));
    }
    public static async Task FollowUpAsync(this InteractionContext context, string content, DiscordEmbed embed)
    {
        await context.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed).WithContent(content));
    }
    
    public static async Task FollowUpAsync(this ContextMenuContext context, string content, DiscordEmbed embed)
    {
        await context.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed).WithContent(content));
    }
}

