using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modmail.Data;
using Modmail.Data.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Modmail.LogViewer.Controllers;

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
}