using System.Reflection;
using Microsoft.OpenApi;
using Timebash.API.Swagger.Filters;

namespace Timebash.API.Extensions;

public static class SwaggerConfiguration
{
    public static IServiceCollection AddSwagger(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Timebash API",
                Version = "v1",
                Description = "API for tracking daily activities and viewing statistics. " +
                    "Supports journals, categories, time correction, and JWT authentication.\n\n" +
                    "⚠️ Timebash API MVP — under active development. Breaking changes may occur without notice.",
                Contact = new OpenApiContact
                {
                    Name = "Kirill Vorobev",
                    Email = "i@kvorobey.ru",
                    Url = new Uri("https://github.com/prosto-vorobey")
                },
            });

            options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. Enter your token"
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("bearer", document)] = []
            });

            options.SchemaFilter<ProblemDetailsSchemaFilter>();

            var apiXmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var apiXmlPath = Path.Combine(AppContext.BaseDirectory, apiXmlFile);
            options.IncludeXmlComments(apiXmlPath);

            var coreXmlPath = Path.Combine(AppContext.BaseDirectory, "Timebash.Core.xml");
            options.IncludeXmlComments(coreXmlPath);
        });

        return services;
    }
}
