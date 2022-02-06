using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Modmail.LogViewer.Controllers;

[Route("manage")]
// [Authorize(AuthenticationSchemes = "Discord")]
public class ManageServerController : Controller
{
    public IActionResult Index()
    {
        Log.Information("ManageServerController.Index");
        return View();
    }
}