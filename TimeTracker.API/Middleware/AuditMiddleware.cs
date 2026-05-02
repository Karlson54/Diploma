using System.Diagnostics;
using System.Text;
using System.Text.Json;
using TimeTracker.Core.Services.Audit;

public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditMiddleware> _logger;

    // Endpoints які НЕ потрібно логувати
    private static readonly HashSet<string> _excludedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/health",
        "/swagger",
    };

    // HTTP методи які логуємо
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
        if (!ShouldLog(context))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;
        var requestBody = await ReadRequestBodyAsync(context.Request);

        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);
            stopwatch.Stop();

            // Копіюємо response назад
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            context.Response.Body = originalBodyStream;

            // ВИПРАВЛЕННЯ: Логуємо помилки ДЛЯ ВСІХ (навіть неавторизованих)
            await LogErrorAsync(context, auditService, ex, requestBody, stopwatch.ElapsedMilliseconds);

            throw; // Пробрасуємо exception далі
        }
    }

    private async Task LogErrorAsync(
        HttpContext context,
        IAuditService auditService,
        Exception exception,
        string? requestBody,
        long elapsedMs)
    {
        try
        {
            var userId = GetUserId(context);
            var userName = GetUserName(context);

            // ВИПРАВЛЕННЯ: Логуємо навіть якщо userId == null
            // Для неавторизованих використовуємо userId = 0
            var actualUserId = userId ?? 0;
            var actualUserName = userName ?? "Anonymous";

            var errorData = new
            {
                ExceptionType = exception.GetType().Name,
                Message = exception.Message,
                Method = context.Request.Method,
                Path = context.Request.Path.Value,
                RequestBody = requestBody,
                ElapsedMs = elapsedMs,
                IsAuthenticated = context.User.Identity?.IsAuthenticated ?? false
            };

            var ipAddress = GetIpAddress(context);
            var userAgent = GetUserAgent(context);

            // Логуємо в БД
            await auditService.LogCreateAsync(
                entityName: "Error",
                entityId: 0,
                newValues: errorData,
                userId: actualUserId,
                userName: actualUserName,
                ipAddress: ipAddress,
                userAgent: userAgent);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log error audit");
        }
    }

    private bool ShouldLog(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        if (_excludedPaths.Any(excluded => path.StartsWith(excluded, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        return _loggedMethods.Contains(context.Request.Method);
    }

    private async Task<string?> ReadRequestBodyAsync(HttpRequest request)
    {
        if (!request.ContentLength.HasValue || request.ContentLength.Value == 0)
        {
            return null;
        }

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
            request.Body.Position = 0;
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
            var jsonDocument = JsonDocument.Parse(body);
            var root = jsonDocument.RootElement;

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

            if (hasSensitiveData)
            {
                return "[SENSITIVE DATA HIDDEN]";
            }

            return body.Length > 2000 ? body.Substring(0, 2000) + "..." : body;
        }
        catch
        {
            return body.Length > 500 ? body.Substring(0, 500) + "..." : body;
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
               ?? "Anonymous";
    }

    private string GetIpAddress(HttpContext context)
    {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (ips.Length > 0)
            {
                return ips[0].Trim();
            }
        }

        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private string GetUserAgent(HttpContext context)
    {
        var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault();

        if (string.IsNullOrEmpty(userAgent))
        {
            return "Unknown";
        }

        return userAgent.Length > 500 ? userAgent.Substring(0, 500) : userAgent;
    }
}