using Microsoft.AspNetCore.DataProtection;

namespace TimeTracker.API.Extensions;

public static class DataProtectionExtensions
{
    public static IServiceCollection AddTimeTrackerDataProtection(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var keysPath = configuration["DataProtection:KeysPath"]
                       ?? Path.Combine(environment.ContentRootPath, "keys");

        services.AddDataProtection()
            .SetApplicationName("TimeTracker")
            .PersistKeysToFileSystem(new DirectoryInfo(keysPath));

        return services;
    }
}