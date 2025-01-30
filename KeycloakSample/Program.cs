using System.Security.Claims;
using KeycloakSample;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGenWithAuth(builder.Configuration);

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/me", (ClaimsPrincipal user) =>
{
    return user.Claims.ToDictionary(c => c.Type, c => c.Value);
}).WithOpenApi().WithName("me").RequireAuthorization();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
