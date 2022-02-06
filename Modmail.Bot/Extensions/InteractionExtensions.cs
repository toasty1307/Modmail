using DSharpPlus.Entities;

namespace Modmail.Bot.Extensions;

public static class InteractionExtensions
{ 
    public static async Task FollowUpAsync(this DiscordInteraction interaction, string content)
    {
        await interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().WithContent(content));
    }

    public static async Task FollowUpAsync(this DiscordInteraction interaction, DiscordEmbed embed)
    {
        await interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed));
    }
    
    public static async Task FollowUpAsync(this DiscordInteraction interaction, string content, DiscordEmbed embed)
    {
        await interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed).WithContent(content));
    }
}