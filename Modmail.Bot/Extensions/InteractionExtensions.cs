using DSharpPlus.Entities;

namespace Modmail.Bot.Extensions;

public static class InteractionExtensions
{ 
    public static Task FollowUpAsync(this DiscordInteraction interaction, string content)
    {
        return interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().WithContent(content));
    }

    public static Task FollowUpAsync(this DiscordInteraction interaction, DiscordEmbed embed)
    {
        return interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed));
    }
    
    public static Task FollowUpAsync(this DiscordInteraction interaction, string content, DiscordEmbed embed)
    {
        return interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed).WithContent(content));
    }
}