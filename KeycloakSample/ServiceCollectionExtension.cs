using Microsoft.OpenApi.Models;

namespace KeycloakSample;

public static class ServiceCollectionExtension
{
    public static void AddSwaggerGenWithAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSwaggerGen(r =>
        {
            r.CustomSchemaIds(s => s.FullName!.Replace('+', '-'));
            r.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "use api `Account/Login` get api token. value pattern: `Bearer {apiToken}`",
            });

            r.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                    },
                    new List<string>()
                }
            });
        });
    }
}
