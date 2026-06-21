namespace TimeTracker.Core.Common;

public static class SystemRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string Employee = "Employee";

    public static readonly string[] All = { SuperAdmin, Admin, Employee };

    public static bool IsSystemRole(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            return false;

        return All.Contains(roleName, StringComparer.OrdinalIgnoreCase);
    }

    public static string GetRoleDescription(string roleName)
    {
        return roleName switch
        {
            SuperAdmin => "Супер адміністратор з повним доступом до системи",
            Admin => "Адміністратор з доступом до дозволених агенцій та відділів",
            Employee => "Співробітник з базовими правами",
            _ => "Користувацька роль"
        };
    }
}