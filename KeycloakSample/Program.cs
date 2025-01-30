using System.Security.Claims;
using Flurl;
using KeycloakSample;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGenWithAuth(builder.Configuration);

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(r =>
    {
        r.RequireHttpsMetadata = false;
        r.Audience = builder.Configuration["Authentication:Audience"];
        r.MetadataAddress = builder.Configuration["Authentication:MetadataAddress"]!;
        r.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["Authentication:ValidIssuer"],
        };
    });

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/auth", ([FromServices] IConfiguration configuration) =>
    {
        var authUrl = configuration["Keycloak:AuthorizationUrl"]!;
        var appUrl = "http://localhost:5162";
        var url = authUrl.SetQueryParams(new
        {
            client_id = "MyClient",
            response_type = "code",
            redirect_uri = appUrl.AppendPathSegment("callback"),
            scope = "openid",
            state = Guid.NewGuid().ToString("n")
        });
        return url.ToString();
    })
    .WithSummary("1. 產生auth url, 使用者操作後跳轉到callback");

app.MapGet("/callback", (string code, string state) =>
{
    return new
    {
        Code = code,
        State = state
    };
}).WithSummary("2. keycloak返回 code和state, 用code呼叫getToken取得token");

app.MapGet("/getToken", async (
    string code,
    [FromServices] IConfiguration configuration) =>
{
    var client = new HttpClient();
    var url = configuration["Keycloak:Url"]!
        .AppendPathSegments("realms", "MyRealm", "protocol", "openid-connect", "token");
    var response = await client.PostAsync(url, new FormUrlEncodedContent(new Dictionary<string, string>
    {
        { "grant_type", "authorization_code" },
        { "code", code },
        { "redirect_uri", "http://localhost:5162/callback" },
        { "client_id", "MyClient" },
    }));
    return await response.Content.ReadAsStringAsync();
}).WithSummary("3. token 有效期限30~60秒, 返回access_token和refresh_token");

app.MapGet("/me", (ClaimsPrincipal user) =>
{
    return user.Claims.ToDictionary(c => c.Type, c => c.Value);
}).WithOpenApi().WithName("me").RequireAuthorization()
.WithSummary("4. getToken取得的token可以直接在這邊使用");

app.UseAuthentication();
app.UseAuthorization();

app.Run();
