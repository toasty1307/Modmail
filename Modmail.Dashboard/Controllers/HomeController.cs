using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modmail.Dashboard.Models;

namespace Modmail.Dashboard.Controllers;

[Route("/")]
public class HomeController : Controller
{
    public IConfiguration _configuration;

    public HomeController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

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
        return Redirect(_configuration["Bot:StatusUrl"]);
    }

    [Route("forbidden")]
    public IActionResult Forbidden()
    {
        return View();
    }

    [Route("invite")]
    public IActionResult Invite()
    {
        return Redirect(_configuration["Bot:InviteLink"]);
    }

    [Route("commands")]
    public IActionResult Commands()
    {
        return View();
    }
    
    [Route("server")]
    public IActionResult Server()
    {
        return Redirect(_configuration["Bot:ServerInvite"]);
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