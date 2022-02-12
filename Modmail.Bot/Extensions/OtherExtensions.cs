using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using DSharpPlus;
using DSharpPlus.Entities;
using Modmail.Bot.Stuff;

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

    public static async Task<Stream> GetStream(this Uri uri)
    {
        using var client = new HttpClient();
        return await client.GetStreamAsync(uri);
    }

    public static async Task<byte[]> GetBytes(this Uri uri)
    {
        return (await uri.GetStream())
            .ReadFully();
    }
    
    public static byte[] ReadFully(this Stream input)
    {
        using var ms = new MemoryStream();
        input.CopyTo(ms);
        return ms.ToArray();
    }
    
    public static ServiceContainer AddService<T>(this ServiceContainer container, T service)
    {
        container.AddService(typeof(T), service!);
        return container;
    }

    public static async Task<DiscordWebhook> GetModmailWebhook(this DiscordChannel channel)
    {
        return (await channel.GetWebhooksAsync())
               .FirstOrDefault(x => x.Name == "Modmail") ??
               await channel.CreateWebhookAsync("Modmail");
    }
}