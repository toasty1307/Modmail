using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modmail.Data;
using Modmail.Data.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Modmail.Dashboard.Controllers;

[Route("manage")]
[Authorize(AuthenticationSchemes = "Discord")]
public class ManageServerController : Controller
{
    private GuildContext _database;
    private readonly IConfiguration _configuration;

    public ManageServerController(GuildContext database, IConfiguration configuration)
    {
        _database = database;
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        return View();
    }

    [Route("{guildId}")]
    public async Task<IActionResult> Server([FromRoute] string guildId)
    {
        if (!Check(guildId, out var guildEntity))
            return Redirect("/forbidden");
        ViewData["GuildId"] = guildId;
        ViewData["GuildName"] = guildEntity.Name;
        ViewData["GuildEntity"] = guildEntity;
        return View();
    }

    [Route("{guildId}/logs")]
    public async Task<IActionResult> Logs([FromRoute] string guildId)
    {
        if (!Check(guildId, out var guildEntity))
            return Redirect("/forbidden");
        ViewData["GuildId"] = guildId;
        ViewData["GuildName"] = guildEntity.Name;
        ViewData["GuildEntity"] = guildEntity;
        return View();
    }

    [NonAction]
    public bool Check(string guildId, out GuildEntity guildEntity)
    {
        guildEntity = null;
        var request = new HttpRequestMessage(HttpMethod.Get,
            $"https://discord.com/api/guilds/{guildId}/members/{User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value}");
        var client = new HttpClient();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bot", _configuration["Bot:Token"]);
        var response = client.Send(request);
        if (!response.IsSuccessStatusCode)
            return false;
        var json =
            JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Content.ReadAsStringAsync().Result);
        var roles = (json?["roles"] as JArray)!.ToObject<string[]>();
        var ge = _database.Guilds.Include(x => x.Config).FirstOrDefault(x => x.Id.ToString() == guildId);
        if (roles == null || !roles.Any() || !roles.Contains(ge!.Config.LogAccessRoleId.ToString()))
            return false;
        guildEntity = ge;
        return true;
    }
}