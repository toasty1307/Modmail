using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modmail.LogViewer.Models;

namespace Modmail.LogViewer.Controllers;

[Route("/")]
public class HomeController : Controller
{
    [Route("/")]
    public IActionResult Index()
    {
        return View();
    }

    [Route("signin")]
    [Authorize(AuthenticationSchemes = "Discord")]
    public IActionResult Login()
    {
        return Redirect("/");
    }

    [Route("status")]
    public IActionResult Status()
    {
        return Redirect("https://modmailstatus.statuspage.io/");
    }

    [Route("forbidden")]
    public IActionResult Forbidden()
    {
        return View();
    }

    [Route("invite")]
    public IActionResult Invite()
    {
        return Redirect(
            "https://discord.com/api/oauth2/authorize?client_id=906162583877333053&permissions=8&scope=bot%20applications.commands");
    }

    [Route("commands")]
    public IActionResult Commands()
    {
        return View();
    }
    
    [Route("server")]
    public IActionResult Server()
    {
        return Redirect(
            "https://discord.gg/tKkGQ6mjzP");
    }
    
    [Route("signout")]
    public IActionResult Logout()
    {
        HttpContext.SignOutAsync();
        return Redirect("/");
    }

    [Route("/error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
    }
}