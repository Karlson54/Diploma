using TimeTracker.API.Extensions;
using TimeTracker.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// API Configuration
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger
builder.Services.AddSwaggerWithJwtAuth();

// Authentication & Authorization
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddTimeTrackerAuthorization();

// Security
builder.Services.AddTimeTrackerCors(builder.Configuration);
builder.Services.AddTimeTrackerRateLimiting();
builder.Services.AddTimeTrackerDataProtection(
    builder.Configuration,
    builder.Environment);

// Application Services
builder.Services.AddTimeTrackerServices();

// Database
builder.Services.AddTimeTrackerDatabase(builder.Configuration, builder.Environment);

var app = builder.Build();

// Seed
await app.SeedDatabaseAsync();

// Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerWithUI();
}

app.UseHttpsRedirection();

// CSP
app.UseMiddleware<SecurityHeadersMiddleware>();

// ExceptionMiddleware
app.UseMiddleware<ExceptionMiddleware>();

// CORS
app.UseTimeTrackerCors();

// RateLimiter
app.UseRateLimiter();

// Authentication
app.UseAuthentication();

// AuditMiddleware
app.UseMiddleware<AuditMiddleware>();

// Authorization
app.UseAuthorization();

// Controllers
app.MapControllers();

app.Run();