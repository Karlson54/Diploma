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
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 1️⃣ ExceptionMiddleware - перший!
app.UseMiddleware<ExceptionMiddleware>();

// 2️⃣ Authentication
app.UseAuthentication();

// 3️⃣ AuditMiddleware
app.UseMiddleware<AuditMiddleware>();

// 4️⃣ Authorization
app.UseAuthorization();

// 5️⃣ Controllers
app.MapControllers();

app.Run();