using System.Diagnostics;
using System.Text;
using System.Text.Json;
using TimeTracker.Core.Services.Audit;

namespace TimeTracker.API.Middleware;

public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditMiddleware> _logger;

    // Endpoints які НЕ потрібно логувати (щоб не засмічувати БД)
    private static readonly HashSet<string> _excludedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/health",
        "/swagger",
        "/api/audit", // Не логуємо запити до самого аудиту
    };

    // HTTP методи які логуємо (тільки зміни)
    private static readonly HashSet<string> _loggedMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "POST",
        "PUT",
        "PATCH",
        "DELETE"
    };

    public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAuditService auditService)
    {
        // Пропускаємо GET запити та excluded paths
        if (!ShouldLog(context))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;

        try
        {
            // Зберігаємо request body для логування
            var requestBody = await ReadRequestBodyAsync(context.Request);

            // Створюємо тимчасовий stream для response
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            // Виконуємо наступний middleware
            await _next(context);

            stopwatch.Stop();

            // Логуємо тільки якщо користувач аутентифікований
            if (context.User.Identity?.IsAuthenticated == true)
            {
                await LogAuditAsync(
                    context,
                    auditService,
                    requestBody,
                    stopwatch.ElapsedMilliseconds);
            }

            // Копіюємо response назад
            await responseBody.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Логуємо помилку
            if (context.User.Identity?.IsAuthenticated == true)
            {
                await LogErrorAsync(context, auditService, ex);
            }

            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private bool ShouldLog(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Не логуємо excluded paths
        if (_excludedPaths.Any(excluded => path.StartsWith(excluded, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        // Логуємо тільки POST, PUT, PATCH, DELETE
        return _loggedMethods.Contains(context.Request.Method);
    }

    private async Task<string?> ReadRequestBodyAsync(HttpRequest request)
    {
        if (!request.ContentLength.HasValue || request.ContentLength.Value == 0)
        {
            return null;
        }

        // Дозволяємо читати body кілька разів
        request.EnableBuffering();

        try
        {
            using var reader = new StreamReader(
                request.Body,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 1024,
                leaveOpen: true);

            var body = await reader.ReadToEndAsync();

            // Повертаємо stream на початок
            request.Body.Position = 0;

            // Не зберігаємо паролі
            return SanitizeRequestBody(body);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read request body for audit");
            return null;
        }
    }

    private string? SanitizeRequestBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            // Парсимо JSON
            var jsonDocument = JsonDocument.Parse(body);
            var root = jsonDocument.RootElement;

            // Перевіряємо чи є чутливі поля
            var sensitiveFields = new[] { "password", "confirmPassword", "currentPassword", "newPassword" };
            var hasSensitiveData = false;

            if (root.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in root.EnumerateObject())
                {
                    if (sensitiveFields.Contains(property.Name, StringComparer.OrdinalIgnoreCase))
                    {
                        hasSensitiveData = true;
                        break;
                    }
                }
            }

            // Якщо є чутливі дані - повертаємо null
            if (hasSensitiveData)
            {
                return "[SENSITIVE DATA HIDDEN]";
            }

            // Обмежуємо розмір (максимум 2000 символів)
            return body.Length > 2000 ? body.Substring(0, 2000) + "..." : body;
        }
        catch
        {
            // Якщо не JSON - повертаємо як є (обмежено)
            return body.Length > 500 ? body.Substring(0, 500) + "..." : body;
        }
    }

    private async Task LogAuditAsync(
        HttpContext context,
        IAuditService auditService,
        string? requestBody,
        long elapsedMs)
    {
        try
        {
            var userId = GetUserId(context);
            var userName = GetUserName(context);

            if (!userId.HasValue)
            {
                return;
            }

            var (action, entityName, entityId) = ParseRouteInfo(context);

            var auditData = new
            {
                Method = context.Request.Method,
                Path = context.Request.Path.Value,
                QueryString = context.Request.QueryString.Value,
                RequestBody = requestBody,
                StatusCode = context.Response.StatusCode,
                ElapsedMs = elapsedMs
            };

            var ipAddress = GetIpAddress(context);
            var userAgent = GetUserAgent(context);

            // Логуємо відповідно до типу операції
            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
            {
                switch (context.Request.Method.ToUpper())
                {
                    case "POST":
                        // Не логуємо create тут - це робиться в сервісах
                        break;
                    case "PUT":
                    case "PATCH":
                        // Не логуємо update тут - це робиться в сервісах
                        break;
                    case "DELETE":
                        // Не логуємо delete тут - це робиться в сервісах
                        break;
                }
            }

            // Можна додати загальне логування для моніторингу
            _logger.LogInformation(
                "Audit: {Method} {Path} by User {UserId} - {StatusCode} ({ElapsedMs}ms)",
                context.Request.Method,
                context.Request.Path,
                userId,
                context.Response.StatusCode,
                elapsedMs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log audit information");
        }
    }

    private async Task LogErrorAsync(
        HttpContext context,
        IAuditService auditService,
        Exception exception)
    {
        try
        {
            var userId = GetUserId(context);
            var userName = GetUserName(context);

            if (!userId.HasValue)
            {
                return;
            }

            _logger.LogError(
                exception,
                "Error in {Method} {Path} by User {UserId}: {Message}",
                context.Request.Method,
                context.Request.Path,
                userId,
                exception.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log error audit");
        }
    }

    private long? GetUserId(HttpContext context)
    {
        var userIdClaim = context.User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
        {
            return null;
        }
        return userId;
    }

    private string GetUserName(HttpContext context)
    {
        return context.User.FindFirst("userName")?.Value 
            ?? context.User.Identity?.Name 
            ?? "Unknown";
    }

    private string GetIpAddress(HttpContext context)
    {
        // Перевіряємо X-Forwarded-For (якщо за proxy/load balancer)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (ips.Length > 0)
            {
                return ips[0].Trim();
            }
        }

        // Перевіряємо X-Real-IP
        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Використовуємо RemoteIpAddress
        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private string GetUserAgent(HttpContext context)
    {
        var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault();
        if (string.IsNullOrEmpty(userAgent))
        {
            return "Unknown";
        }

        // Обмежуємо розмір
        return userAgent.Length > 500 ? userAgent.Substring(0, 500) : userAgent;
    }

    private (string action, string entityName, long? entityId) ParseRouteInfo(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        var method = context.Request.Method.ToUpper();

        // Намагаємось визначити entity та action з route
        // Приклад: /api/users/123 -> Users, 123
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length < 2)
        {
            return (method, "Unknown", null);
        }

        var entityName = segments.Length > 1 ? segments[1] : "Unknown";
        long? entityId = null;

        if (segments.Length > 2 && long.TryParse(segments[2], out var id))
        {
            entityId = id;
        }

        var action = method switch
        {
            "POST" => "Create",
            "PUT" => "Update",
            "PATCH" => "Update",
            "DELETE" => "Delete",
            _ => "Read"
        };

        return (action, entityName, entityId);
    }
}