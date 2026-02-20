using Microsoft.AspNetCore.Authorization;

namespace TimeTracker.API.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddTimeTrackerAuthorization(this IServiceCollection services)
    {
        var authBuilder = services.AddAuthorizationBuilder();

        // Базовые политики
        AddBasicPolicies(authBuilder);

        // Политики для управления пользователями
        AddUserManagementPolicies(authBuilder);

        // Политики для справочников
        AddDictionaryPolicies(authBuilder);

        // Политики для записей времени
        AddTimeEntryPolicies(authBuilder);

        // Политики для отчётов
        AddReportingPolicies(authBuilder);

        return services;
    }

    private static void AddBasicPolicies(AuthorizationBuilder builder)
    {
        builder
            .AddPolicy("AdminOnly", policy =>
                policy.RequireRole("Admin"))
            .AddPolicy("ManagerOrAdmin", policy =>
                policy.RequireRole("Manager", "Admin"))
            .AddPolicy("AuthenticatedUser", policy =>
                policy.RequireAuthenticatedUser());
    }

    private static void AddUserManagementPolicies(AuthorizationBuilder builder)
    {
        builder
            .AddPolicy("CanManageUsers", policy =>
                policy.RequireRole("Admin")
                    .RequireClaim("IsActive", "True"))
            .AddPolicy("CanViewUsers", policy =>
                policy.RequireRole("Admin", "Manager"));
    }

    private static void AddDictionaryPolicies(AuthorizationBuilder builder)
    {
        builder
            .AddPolicy("CanEditDictionaries", policy =>
                policy.RequireAssertion(context =>
                    context.User.IsInRole("Admin") ||
                    context.User.IsInRole("Manager")))
            .AddPolicy("CanViewDictionaries", policy =>
                policy.RequireAuthenticatedUser())
            .AddPolicy("CanManageDictionaries", policy =>
                policy.RequireAssertion(context =>
                    context.User.IsInRole("Admin")));
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
                policy.RequireRole("Manager", "Admin"));
    }

    private static void AddReportingPolicies(AuthorizationBuilder builder)
    {
        builder
            .AddPolicy("CanViewReports", policy =>
                policy.RequireRole("Manager", "Admin", "Accountant"))
            .AddPolicy("CanExportData", policy =>
                policy.RequireRole("Admin", "Accountant"))
            .AddPolicy("CanViewFinancials", policy =>
                policy.RequireRole("Accountant", "Admin"))
            .AddPolicy("CanViewOwnReports", policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim("IsActive", "True"))
            .AddPolicy("CanViewAllReports", policy =>
                policy.RequireRole("Admin", "Manager"));
    }
}