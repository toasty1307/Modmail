using DSharpPlus;
using Modmail.Bot.BotExtensions;

namespace Modmail.Bot.Extensions;

public static class OtherExtensions
{
    public static ModmailExtension GetModmailExtension(this DiscordClient client)
    {
        return client.GetExtension<ModmailExtension>();
    }
}