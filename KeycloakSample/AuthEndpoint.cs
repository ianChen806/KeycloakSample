using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace KeycloakSample;

public static class AuthEndpoint
{
    public static void AddAuthEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("auth");

        group.MapGet("login", () =>
        {
            var schemes = new List<string>() { OpenIdConnectDefaults.AuthenticationScheme };
            var properties = new AuthenticationProperties
            {
                RedirectUri = "/"
            };
            return Results.Challenge(properties, schemes);
        });

        group.MapGet("logout", async (IHttpContextAccessor contextAccessor) =>
        {
            var context = contextAccessor.HttpContext!;
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
            return Results.Redirect("/");
        });

        group.MapGet("me", (ClaimsPrincipal user) =>
            {
                return user.Claims.ToDictionary(c => c.Type, c => c.Value);
            })
            .WithOpenApi()
            .WithSummary("user info")
            .RequireAuthorization();

        group.MapGet("callback", () =>
        {
            Console.WriteLine("callback");
        });

        group.MapGet("signedOut", () =>
        {
            Console.WriteLine("signedOut");
        });
    }
}
