using System.Security.Claims;
using KeycloakSample.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGenWithAuth(builder.Configuration);

builder.Services.AddAuthorization();
builder.Services.AddAuthenticationWithJwt(builder.Configuration);

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.AddAuthEndpoints();
app.MapGet("me", (ClaimsPrincipal user) =>
    {
        return user.Claims.ToDictionary(c => c.Type, c => c.Value);
    })
    .WithOpenApi()
    .WithSummary("user info")
    .RequireAuthorization();

app.Map("/", () => "ok");
app.Run();
