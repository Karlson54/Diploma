using Microsoft.AspNetCore.Authorization;

namespace TimeTracker.API.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddTimeTrackerAuthorization(this IServiceCollection services)
    {
        var authBuilder = services.AddAuthorizationBuilder();

        AddBasicPolicies(authBuilder);
        AddUserManagementPolicies(authBuilder);
        AddDictionaryPolicies(authBuilder);
        AddTimeEntryPolicies(authBuilder);
        AddReportingPolicies(authBuilder);

        return services;
    }

    private static void AddBasicPolicies(AuthorizationBuilder builder)
    {
        builder
            .AddPolicy("SuperAdminOnly", policy =>
                policy.RequireRole("SuperAdmin"))
            .AddPolicy("AdminOrSuperAdmin", policy =>
                policy.RequireRole("Admin", "SuperAdmin"))
            .AddPolicy("AuthenticatedUser", policy =>
                policy.RequireAuthenticatedUser());
    }

    private static void AddUserManagementPolicies(AuthorizationBuilder builder)
    {
        builder
            .AddPolicy("CanManageUsers", policy =>
                policy.RequireRole("SuperAdmin")
                    .RequireClaim("IsActive", "True"))
            .AddPolicy("CanViewUsers", policy =>
                policy.RequireRole("SuperAdmin", "Admin"));
    }

    private static void AddDictionaryPolicies(AuthorizationBuilder builder)
    {
        builder
            .AddPolicy("CanEditDictionaries", policy =>
                policy.RequireAssertion(context =>
                    context.User.IsInRole("SuperAdmin") ||
                    context.User.IsInRole("Admin")))
            .AddPolicy("CanViewDictionaries", policy =>
                policy.RequireAuthenticatedUser())
            .AddPolicy("CanManageDictionaries", policy =>
                policy.RequireAssertion(context =>
                    context.User.IsInRole("SuperAdmin")));
    }

    private static void AddTimeEntryPolicies(AuthorizationBuilder builder)
    {
        builder
            .AddPolicy("CanCreateTimeEntry", policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim("IsActive", "True"))
            .AddPolicy("CanEditOwnTimeEntry", policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim("IsActive", "True"))
            .AddPolicy("CanDeleteOwnTimeEntry", policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim("IsActive", "True"))
            .AddPolicy("CanEditAnyTimeEntry", policy =>
                policy.RequireRole("Admin", "SuperAdmin"));
    }

    private static void AddReportingPolicies(AuthorizationBuilder builder)
    {
        builder
            .AddPolicy("CanViewReports", policy =>
                policy.RequireRole("Admin", "SuperAdmin"))
            .AddPolicy("CanExportData", policy =>
                policy.RequireRole("Admin", "SuperAdmin"))
            .AddPolicy("CanViewOwnReports", policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim("IsActive", "True"))
            .AddPolicy("CanViewAllReports", policy =>
                policy.RequireRole("Admin", "SuperAdmin"))
            // Назначение прав доступа — только SuperAdmin
            .AddPolicy("CanManageAdminPermissions", policy =>
                policy.RequireRole("SuperAdmin"));
    }
}