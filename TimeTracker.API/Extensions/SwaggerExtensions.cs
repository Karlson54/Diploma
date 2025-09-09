using System.Reflection;
using Microsoft.OpenApi.Models;

namespace TimeTracker.API.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerWithJwtAuth(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            // Основная информация об API
            options.SwaggerDoc("v1", new OpenApiInfo 
            { 
                Title = "TimeTracker API", 
                Version = "v1",
                Description = "API для системы учета рабочего времени",
                Contact = new OpenApiContact
                {
                    Name = "TimeTracker Support",
                    Email = "support@timetracker.com"
                }
            });

            // Определение схемы авторизации JWT
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Введите JWT токен в формате: Bearer {ваш-токен}"
            });

            // Требование авторизации для всех endpoints
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Включение XML документации (опционально)
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }
}