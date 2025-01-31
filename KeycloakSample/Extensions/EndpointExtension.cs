using Flurl;
using Microsoft.AspNetCore.Mvc;

namespace KeycloakSample.Extensions;

public static class EndpointExtension
{
    public static void AddAuthEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/auth");
        Auth(group);
        Callback(group);
        GetToken(group);
        Refresh(group);
    }

    private static void Auth(RouteGroupBuilder group)
    {
        group.MapPost("/", ([FromServices] IConfiguration configuration) =>
            {
                var authUrl = configuration["Keycloak:AuthorizationUrl"]!;
                var url = authUrl.SetQueryParams(new
                {
                    client_id = configuration["Keycloak:ClientId"],
                    response_type = "code",
                    redirect_uri = CallbackUrl(configuration),
                    scope = "openid",
                    state = Guid.NewGuid().ToString("n")
                });
                return url.ToString();
            })
            .WithSummary("1. 產生auth url, 使用者登入成功後 keycloak 會重新導向到 redirect uri(callback)");
    }

    private static string CallbackUrl(IConfiguration configuration)
    {
        return configuration["AppUrl"]!.AppendPathSegments("auth", "callback").ToString();
    }

    private static void Callback(RouteGroupBuilder group)
    {
        group.MapGet("/callback", (string code, string state) =>
            {
                return new CallbackDto
                {
                    Code = code,
                    State = state
                };
            })
            .WithSummary("2. 使用者登入成功後, keycloak返回code和state");
    }

    private static void GetToken(RouteGroupBuilder group)
    {
        group.MapGet("/token", async (string code, [FromServices] IConfiguration configuration) =>
            {
                using var client = new HttpClient();
                var realm = configuration["Keycloak:Realm"]!;
                var tokenUrl = configuration["Keycloak:Url"]!.AppendPathSegments("realms", realm, "protocol", "openid-connect", "token")!;
                var response = await client.PostAsync(tokenUrl, new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "authorization_code" },
                    { "code", code },
                    { "redirect_uri", CallbackUrl(configuration) },
                    { "client_id", "MyClient" },
                }));
                return await response.Content.ReadAsStringAsync();
            })
            .WithSummary("3. 使用callback返回的code 取得 token, code的有效期限30~60秒, 返回access_token和refresh_token");
    }

    private static void Refresh(RouteGroupBuilder group)
    {
        group.MapGet("/refresh", async (string refreshToken, [FromServices] IConfiguration configuration) =>
            {
                using var client = new HttpClient();
                var realm = configuration["Keycloak:Realm"]!;
                var tokenUrl = configuration["Keycloak:Url"]!.AppendPathSegments("realms", realm, "protocol", "openid-connect", "token")!;
                var response = await client.PostAsync(tokenUrl, new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "refresh_token" },
                    { "refresh_token", refreshToken },
                    { "client_id", "MyClient" },
                }));
                return await response.Content.ReadAsStringAsync();
            })
            .WithSummary("x. 拿 RefreshToken 取得新的 token");
    }

    public class CallbackDto
    {
        public string Code { get; set; }

        public string State { get; set; }
    }
}
