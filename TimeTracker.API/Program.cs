using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using TimeTracker.API.Extensions;
using TimeTracker.Data.Context;
using TimeTracker.Data.Repositories.Common;
using TimeTracker.Data.Repositories.Roles;
using TimeTracker.Data.Repositories.TimeEntries;
using TimeTracker.Data.Repositories.Users;
using TimeTracker.Data.UnitOfWork;
using TimeTracker.API.Infrastructure.Options;

var builder = WebApplication.CreateBuilder(args);

// Основные сервисы
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithJwtAuth();

builder.Services.ConfigureOptions<JwtOptionsSetup>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

// Авторизационные политики
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("ManagerOrAdmin", policy =>
        policy.RequireRole("Manager", "Admin"));

    options.AddPolicy("AuthenticatedUser", policy =>
        policy.RequireAuthenticatedUser());

    options.AddPolicy("CanViewReports", policy =>
        policy.RequireClaim("permission", "view_reports"));
});

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();


builder.Services.AddDbContext<TimeTrackerDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("TimeTracker"),
        sqlServerOptions =>
        {
            sqlServerOptions.MigrationsAssembly(
                typeof(TimeTrackerDbContext).Assembly.GetName().Name);
            sqlServerOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });

    if (!builder.Environment.IsDevelopment()) return;
    options.EnableDetailedErrors();
    options.EnableSensitiveDataLogging();
});

builder.Services.AddScoped<ITimeTrackerDbContext>(provider =>
    provider.GetRequiredService<TimeTrackerDbContext>());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();