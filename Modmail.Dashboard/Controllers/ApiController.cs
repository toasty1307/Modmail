using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modmail.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Modmail.Dashboard.Controllers;

public record GuildData(string Name, string Icon, string Id);

[ApiController]
[Route("api")]
public class ApiController : ControllerBase
{
    private GuildContext _database;
    private readonly IConfiguration _configuration;

    public ApiController(GuildContext database, IConfiguration configuration)
    {
        _database = database;
        _configuration = configuration;
    }

    [HttpGet("@me/guilds")]
    public async Task<IActionResult> UserGuilds()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return Unauthorized();
        var guildsString = User.Claims.FirstOrDefault(x => x.Type == "Guilds")?.Value ?? "";
        if (string.IsNullOrEmpty(guildsString))
            return new JsonResult(new List<GuildData>());
        var userGuildIds = guildsString.Split(';').Select(ulong.Parse);
        var commonGuilds = _database.Guilds
            .Include(x => x.Config)
            .Include(x => x.Icon)
            .Where(x => userGuildIds.Contains(x.Id))
            .ToArray();
        var id = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)!.Value;
        var client = new HttpClient();
        var list = new List<GuildData>();
        foreach (var commonGuild in commonGuilds)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get,
                $"https://discord.com/api/guilds/{commonGuild.Id}/members/{id}");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bot", _configuration["Bot:Token"]);
            var response = await client.SendAsync(requestMessage);
            if (!response.IsSuccessStatusCode)
                continue;
            var roles =
                ((JArray)JsonConvert.DeserializeObject<Dictionary<string, object>>(await response.Content.ReadAsStringAsync())!["roles"]).ToArray();
            if (roles.Contains(commonGuild.Config.LogAccessRoleId.ToString()))
            {
                list.Add(new GuildData(commonGuild.Name, Convert.ToBase64String(commonGuild.Icon.Data), commonGuild.Id.ToString()));
            }
        }
        return new JsonResult(list);
    }
}