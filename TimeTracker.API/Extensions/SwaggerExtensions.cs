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
                Description = """
                              API для системи обліку робочого часу рекламних агентств.

                              **Аутентифікація:**
                              1. Отримайте токен через `POST /api/auth/login`
                              2. Натисніть кнопку **Authorize** вгорі сторінки
                              3. Введіть токен у форматі: `Bearer {ваш-токен}`

                              **Ролі:**
                              - **SuperAdmin** — повний доступ до системи
                              - **Admin** — доступ до агенцій та відділів який дасть супер адмін
                              - **Employee** — створення власних записів часу
                              """,
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
                Description = "Введіть токен у форматі: `Bearer eyJhbGci...`"
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

            // Порядок відображення груп у Swagger UI
            options.TagActionsBy(api => api.GroupName != null
                ? new[] { api.GroupName }
                : new[] { api.ActionDescriptor.RouteValues["controller"] ?? "Other" });

            options.OrderActionsBy(api =>
                $"{GetTagOrder(api.ActionDescriptor.RouteValues["controller"])}{api.RelativePath}");

            // XML Comments — API + Core (там живуть DTO)
            var baseDir = AppContext.BaseDirectory;
            var xmlFiles = new[]
            {
                $"{Assembly.GetExecutingAssembly().GetName().Name}.xml",
                "TimeTracker.Core.xml"
            };

            foreach (var xmlFile in xmlFiles)
            {
                var xmlPath = Path.Combine(baseDir, xmlFile);
                if (File.Exists(xmlPath))
                    options.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }

    public static IApplicationBuilder UseSwaggerWithUI(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "TimeTracker API v1");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "TimeTracker API";

            // Розгортаємо тільки першу групу (Auth) — решта згорнуті
            options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);

            // Показуємо час виконання запиту
            options.DisplayRequestDuration();

            // Сортуємо методи: GET → POST → PUT → PATCH → DELETE
            options.EnableFilter();
        });

        return app;
    }

    // Визначаємо порядок груп у sidebar
    private static string GetTagOrder(string? controller) => controller switch
    {
        "Auth" => "1",
        "Users" => "2",
        "Roles" => "3",
        "TimeEntries" => "4",
        "Reports" => "5",
        "Agencies" => "6",
        "Markets" => "7",
        "ContractingAgencies" => "8",
        "Clients" => "9",
        "ProjectBrands" => "10",
        "Media" => "11",
        "JobTypes" => "12",
        _ => "99"
    };
}