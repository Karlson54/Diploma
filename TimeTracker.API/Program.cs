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

// Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<AuditMiddleware>(); // MIDDLEWARE
app.UseAuthorization();
app.MapControllers();

app.Run();