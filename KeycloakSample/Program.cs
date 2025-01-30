using System.Security.Claims;
using KeycloakSample;
using Microsoft.AspNetCore.Authentication.JwtBearer;

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

app.MapGet("/me", (ClaimsPrincipal user) =>
{
    return user.Claims.ToDictionary(c => c.Type, c => c.Value);
}).WithOpenApi().WithName("me").RequireAuthorization();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
