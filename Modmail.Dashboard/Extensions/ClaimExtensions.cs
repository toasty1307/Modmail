using System.Security.Claims;
using DSharpPlus;
using Modmail.Data.Entities;
using Newtonsoft.Json;

namespace Modmail.Dashboard.Extensions;

public static class ClaimExtensions
{
    public static string? GetClaim(this ClaimsPrincipal identity, string claimType)
    {
        return identity.Claims.FirstOrDefault(x => x.Type == claimType)?.Value;
    }
    
    public static string? GetId(this ClaimsPrincipal identity)
    {
        return identity.GetClaim(ClaimTypes.NameIdentifier);
    }
    
    public static string? GetAvatar(this ClaimsPrincipal identity)
    {
        return "https://cdn.discordapp.com/avatars/" + identity.GetId() + "/" + identity.GetClaim(ClaimTypes.Hash) + ".png";
    }
    
    public static string? GetName(this ClaimsPrincipal identity)
    {
        return identity.GetClaim(ClaimTypes.Name) + "#" + identity.GetClaim(ClaimTypes.Surname);
    }

    public static ulong[] GetGuilds(this ClaimsPrincipal identity)
    {
        return identity.GetClaim("Guilds")?.Split(";").Select(ulong.Parse).ToArray() ?? Array.Empty<ulong>();
    }
    
    public static string? GetToken(this ClaimsPrincipal identity)
    {
        return identity.GetClaim("Token");
    }
    
    public static async Task<bool> CheckManageServerPermission(this ClaimsPrincipal identity, GuildEntity? entity)
    {
        if (entity is null)
            return false;
        
        var token = identity.GetToken();
        if (token is null)
            return false;
        
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://discord.com/api/users/@me/guilds");
        request.Headers.Add("Authorization", "Bearer " + token);
        using var response = await _client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return false;
        
        var json = await response.Content.ReadAsStringAsync();
        
        var guilds = JsonConvert.DeserializeObject<Guild[]>(json);
        return guilds!.Any(x => x.Id == entity.Id && x.Permissions.HasFlag(Permissions.ManageGuild));
    }
    
    private static readonly HttpClient _client = new();
    public static async Task<GuildEntity[]> GetGuildsWithAccess(this ClaimsPrincipal identity, IEnumerable<GuildEntity> entities)
    {
        var guildEntities = entities as GuildEntity[] ?? entities.ToArray();
        if (!guildEntities?.Any() ?? true)
            return Array.Empty<GuildEntity>();
        
        var token = identity.GetToken();
        if (token is null)
            return Array.Empty<GuildEntity>();
        
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://discord.com/api/users/@me/guilds");
        request.Headers.Add("Authorization", "Bearer " + token);
        using var response = await _client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return Array.Empty<GuildEntity>();
        
        var json = await response.Content.ReadAsStringAsync();
        
        var guilds = JsonConvert.DeserializeObject<Guild[]>(json);
        return guildEntities.Where(x => guilds.Any(y => y.Id == x.Id && y.Permissions.HasFlag(Permissions.ManageGuild))).ToArray();
    }

    private record Guild([JsonProperty("id")] ulong Id, [JsonProperty("permissions")] Permissions Permissions);
}