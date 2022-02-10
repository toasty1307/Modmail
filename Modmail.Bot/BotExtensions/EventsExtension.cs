using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.EventArgs;
using Modmail.Bot.Extensions;
using Serilog;

namespace Modmail.Bot.BotExtensions;

public class EventsExtension : BaseExtension
{
    protected override void Setup(DiscordClient client)
    {
        Client = client;
        client.GuildDownloadCompleted += ClientOnGuildDownloadCompleted;
    }

    private Task ClientOnGuildDownloadCompleted(DiscordClient sender, GuildDownloadCompletedEventArgs e)
    {
        return Task.CompletedTask;
    }

    public async Task SlashCommandErrored(SlashCommandsExtension sender, SlashCommandErrorEventArgs e)
    {
        await OnInteractionErrored(e.Context.Interaction, e.Exception, e.Context.CommandName);
    }

    public async Task ContextMenuErrored(SlashCommandsExtension sender, ContextMenuErrorEventArgs e)
    {
        await OnInteractionErrored(e.Context.Interaction, e.Exception, e.Context.CommandName);
    }
    
    private async Task OnInteractionErrored(DiscordInteraction interaction, Exception exception, string commandName)
    {
        var reply = exception switch
        {
            SlashExecutionChecksFailedException checksException => checksException.FailedChecks[0] switch
            {
                SlashRequireBotPermissionsAttribute => "Missing Bot permissions",
                SlashRequireDirectMessageAttribute => "DM only",
                SlashRequireGuildAttribute => "Guild only",
                SlashRequireOwnerAttribute => "Owner only",
                SlashRequirePermissionsAttribute => "Missing Bot or User permissions",
                SlashRequireUserPermissionsAttribute => "Missing User Permissions",
                _ => throw new ArgumentOutOfRangeException(),
            },
            _ => "Unknown Error",
        };
        try
        {
            Log.Error(exception, "Error in {Command}", commandName);
            var original = await interaction.GetOriginalResponseAsync();
            if (original is null)
            {
                await interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral(true).WithContent(reply));
                original = await interaction.GetOriginalResponseAsync();
                await original.Error();
            }
            else
            {
                await interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent(reply));
                await original.Error();
            }
        }
        catch
        {
            Log.Error(exception, "Error in {Command}", commandName);
            throw; // debugger stop
        }
    }
}