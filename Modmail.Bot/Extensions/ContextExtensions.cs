using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace Modmail.Bot.Extensions;

public static class ContextExtensions
{
    public static Task FollowUpAsync(this InteractionContext context, string message)
    {
        return context.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent(message));
    }
    
    public static Task FollowUpAsync(this ContextMenuContext context, string message)
    {
        return context.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent(message));
    } 
    
    public static Task FollowUpAsync(this InteractionContext context, DiscordEmbed embed)
    {
        return context.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed));
    }
    
    public static Task FollowUpAsync(this ContextMenuContext context, DiscordEmbed embed)
    {
        return context.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed));
    }
    public static Task FollowUpAsync(this InteractionContext context, string content, DiscordEmbed embed)
    {
        return context.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed).WithContent(content));
    }
    
    public static Task FollowUpAsync(this ContextMenuContext context, string content, DiscordEmbed embed) 
    {
        return context.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed).WithContent(content));
    }
    
    public static Task<DiscordMessage> EditResponseAsync(this InteractionContext interaction, string content)
    {
        return interaction.EditResponseAsync(new DiscordWebhookBuilder().WithContent(content));
    }

    public static Task<DiscordMessage> EditResponseAsync(this InteractionContext interaction, DiscordEmbed embed)
    {
        return interaction.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
    }
    
    public static Task<DiscordMessage> EditResponseAsync(this InteractionContext interaction, string content, DiscordEmbed embed)
    {
        return interaction.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).WithContent(content));
    }
    
    public static Task<DiscordMessage> EditResponseAsync(this ContextMenuContext interaction, string content)
    {
        return interaction.EditResponseAsync(new DiscordWebhookBuilder().WithContent(content));
    }

    public static Task<DiscordMessage> EditResponseAsync(this ContextMenuContext interaction, DiscordEmbed embed)
    {
        return interaction.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
    }
    
    public static Task<DiscordMessage> EditResponseAsync(this ContextMenuContext interaction, string content, DiscordEmbed embed)
    {
        return interaction.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).WithContent(content));
    }
}

