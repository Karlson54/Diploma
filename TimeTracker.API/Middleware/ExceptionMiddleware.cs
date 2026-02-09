using System.Text.Json;

namespace TimeTracker.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Unhandled exception. Path: {Path}, Method: {Method}", 
                context.Request.Path, 
                context.Request.Method);

            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var (statusCode, error, message) = exception switch
        {
            UnauthorizedAccessException => 
                (StatusCodes.Status401Unauthorized, "Unauthorized", exception.Message),
                
            KeyNotFoundException => 
                (StatusCodes.Status404NotFound, "Not Found", exception.Message),
                
            InvalidOperationException => 
                (StatusCodes.Status409Conflict, "Conflict", exception.Message),
                
            ArgumentException => 
                (StatusCodes.Status400BadRequest, "Bad Request", exception.Message),
                
            _ => (StatusCodes.Status500InternalServerError, 
                  "Internal Server Error",
                  _environment.IsDevelopment() 
                      ? exception.Message 
                      : "Виникла внутрішня помилка. Спробуйте пізніше.")
        };

        context.Response.StatusCode = statusCode;

        var response = new
        {
            Error = error,
            Message = message,
            StatusCode = statusCode,
            Timestamp = DateTime.UtcNow,
            Path = context.Request.Path.Value
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}