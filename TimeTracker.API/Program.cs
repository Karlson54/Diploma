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

// ExceptionMiddleware
app.UseMiddleware<ExceptionMiddleware>();

// Authentication
app.UseAuthentication();

// AuditMiddleware
app.UseMiddleware<AuditMiddleware>();

// Authorization
app.UseAuthorization();

// Controllers
app.MapControllers();

app.Run();