using DSharpPlus;
using DSharpPlus.Entities;
using Modmail.Bot.BotExtensions;

namespace Modmail.Bot.Extensions;

public static class OtherExtensions
{
    public static ModmailExtension GetModmailExtension(this DiscordClient client)
    {
        return client.GetExtension<ModmailExtension>();
    }

    public static Task Success(this DiscordMessage message)
    {
        return message.CreateReactionAsync(DiscordEmoji.FromUnicode("✅"));
    }
    
    public static Task Error(this DiscordMessage message)
    {
        return message.CreateReactionAsync(DiscordEmoji.FromUnicode("❌"));
    }

    public static async Task<byte[]> GetAvatar(this Uri uri)
    {
        var client = new HttpClient();
        var response = await client.GetStreamAsync(uri);
        var ms = new MemoryStream();
        await response.CopyToAsync(ms);
        return ms.ToArray();
    }
}