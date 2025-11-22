using System.Reflection;
using Microsoft.OpenApi.Models;

namespace TimeTracker.API.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerWithJwtAuth(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "TimeTracker API",
                Version = "v1",
                Description = @"
                API для системи учета рабочего времени рекламных агентств.
                
                **Аутентификація:**
                1. Отримайте токен через `/api/auth/login`
                2. Натисніть кнопку 'Authorize' вгорі
                3. Введіть токен у форматі: `Bearer {ваш-токен}`
                
                **Ролі:**
                - Admin: повний доступ до системи
                - Manager: управління проектами та користувачами
                - Employee: створення власних записів часу
                - Accountant: доступ до фінансових звітів
            ",
                Contact = new OpenApiContact
                {
                    Name = "TimeTracker Support",
                    Email = "support@timetracker.com"
                }
            });

            // JWT Security Definition
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = @"
                JWT Authorization header використовуючи Bearer схему.
                
                Введіть 'Bearer' [пробіл] а потім ваш токен.
                
                Приклад: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`
            "
            });

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

            // XML Comments
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