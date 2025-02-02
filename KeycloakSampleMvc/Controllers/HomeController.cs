using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using KeycloakSampleMvc.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;

namespace KeycloakSampleMvc.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Login()
    {
        return Challenge(new AuthenticationProperties()
        {
            RedirectUri = Url.Action("Index", "Home")
        }, OpenIdConnectDefaults.AuthenticationScheme);
    }

    public async Task<IActionResult> Logout([FromServices] IHttpContextAccessor contextAccessor)
    {
        var context = contextAccessor.HttpContext!;
        await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    public IActionResult Profile()
    {
        return View();
    }

    public IActionResult Callback()
    {
        Console.WriteLine("Callback");
        return RedirectToAction("Index");
    }

    public IActionResult SignedOut()
    {
        Console.WriteLine("SignedOut");
        return RedirectToAction("Index");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
