using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("me", (ClaimsPrincipal user) =>
    {
        return user.Claims.ToDictionary(c => c.Type, c => c.Value);
    })
    .WithOpenApi()
    .WithSummary("user info")
    .RequireAuthorization();

app.Map("/", () => "ok");
app.Run();
