using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace KeycloakSample;

public static class ServiceCollectionExtensions
{
    public static void AddAuthenticationWithOpenIdConnect(this IServiceCollection service, IConfiguration configuration)
    {
        service.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddOpenIdConnect(options =>
            {
                options.RequireHttpsMetadata = false;
                options.Authority = configuration["Authentication:Authority"];
                options.ClientId = configuration["Authentication:ClientId"];
                options.ClientSecret = configuration["Authentication:ClientSecret"];
                options.ResponseType = configuration["Authentication:ResponseType"]!;
                options.SaveTokens = true;
                options.TokenValidationParameters.NameClaimType = "name";

                options.CallbackPath = configuration["Authentication:CallbackPath"];
                options.SignedOutCallbackPath = configuration["Authentication:SignedOutCallbackPath"];

                foreach (var scope in configuration.GetSection("Authentication:Scope").Get<string[]>()!)
                {
                    options.Scope.Add(scope);
                }

                options.Events = new OpenIdConnectEvents()
                {
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine("TokenValidated");
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine("AuthenticationFailed");
                        return Task.CompletedTask;
                    },
                    OnRedirectToIdentityProvider = context =>
                    {
                        if (context.Request.Path != "/auth/login")
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.HandleResponse();
                        }
                        return Task.CompletedTask;
                    }
                };
            });
    }
}
