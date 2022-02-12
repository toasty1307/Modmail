using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modmail.Dashboard.Extensions;
using Modmail.Data;

namespace Modmail.Dashboard.Controllers;

public record GuildData(string Name, string Icon, string Id);
public record LogData(string Name, string Icon, string Id, double Time);

[ApiController]
[Route("api")]
public class ApiController : ControllerBase
{
    private GuildContext _database;

    public ApiController(GuildContext database)
    {
        _database = database;
    }

    [HttpGet("@me/guilds")]
    public async Task<IActionResult> UserGuilds()
    {
        if (!User.Identity!.IsAuthenticated)
            return Unauthorized();
        
        var userGuilds = User.GetGuilds();
        var guilds = _database.Guilds
            .Include(x => x.Icon)
            .Include(x => x.Config)
            .Where(x => userGuilds.Contains(x.Id)).ToList();
        var guildsWithAccess = await User.GetGuildsWithAccess(guilds);
        var list = guildsWithAccess.Select(guildEntity => new GuildData(guildEntity.Name, Convert.ToBase64String(guildEntity.Icon.Data), guildEntity.Id.ToString())).ToList();
        return new JsonResult(list);
    }

    [HttpGet("@me/guilds/{guildIdString}/logs")]
    [AllowAnonymous]
    public async Task<IActionResult> GuildsLogs([FromRoute] string guildIdString)
    {
        if (!User.Identity!.IsAuthenticated)
            return Unauthorized();
        
        if (!ulong.TryParse(guildIdString, out var id))
            return Forbid();
        
        var guildEntity = _database.Guilds
            .Include(x => x.Config)
            .FirstOrDefault(x => x.Id == id);
        var hasAccess = await User.CheckManageServerPermission(guildEntity);
        if (!hasAccess)
            return Forbid();

        var threads = _database.Threads
            .Include(x => x.RecipientAvatar)
            .Where(x => x.GuildId == id);
        var list = new List<LogData>();
        foreach (var threadEntity in threads)
        {
            var time = threadEntity.Created.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            list.Add(new LogData(threadEntity.RecipientName, Convert.ToBase64String(threadEntity.RecipientAvatar.Data), threadEntity.Id.ToString(), time));
        }
        return new JsonResult(list);
    }
}
