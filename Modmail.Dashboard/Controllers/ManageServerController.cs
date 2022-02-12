using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modmail.Dashboard.Extensions;
using Modmail.Data;

namespace Modmail.Dashboard.Controllers;

[Route("manage")]
[Authorize(AuthenticationSchemes = "Discord")]
public class ManageServerController : Controller
{
    private GuildContext _database;
    private readonly ILogger<ManageServerController> _logger;

    public ManageServerController(GuildContext database, ILogger<ManageServerController> logger)
    {
        _database = database;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [Route("{guildId}")]
    public async Task<IActionResult> Server([FromRoute] string guildId)
    {
        if (!ulong.TryParse(guildId, out var id))
            return Redirect("/forbidden");
        
        var guildEntity = _database.Guilds
            .Include(x => x.Config)
            .FirstOrDefault(x => x.Id == id);
        var hasAccess = await User.CheckManageServerPermission(guildEntity);
        if (!hasAccess)
            return Redirect("/forbidden");
        
        ViewData["GuildId"] = guildId;
        ViewData["GuildName"] = guildEntity!.Name;
        ViewData["GuildEntity"] = guildEntity;
        return View();
    }

    [Route("{guildId}/logs")]
    public async Task<IActionResult> Logs([FromRoute] string guildId)
    {
        if (!ulong.TryParse(guildId, out var id))
            return Redirect("/forbidden");
        
        var guildEntity = _database.Guilds
            .Include(x => x.Config)
            .FirstOrDefault(x => x.Id == id);
        var hasAccess = await User.CheckManageServerPermission(guildEntity);
        if (!hasAccess)
            return Redirect("/forbidden");

        ViewData["GuildId"] = guildId;
        ViewData["GuildName"] = guildEntity!.Name;
        ViewData["GuildEntity"] = guildEntity;
        return View();
    }
}