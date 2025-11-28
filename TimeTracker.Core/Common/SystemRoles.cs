namespace TimeTracker.Core.Common;

public static class SystemRoles
{
    public const string Admin = "Admin";
    
    public const string Manager = "Manager";
    
    public const string Employee = "Employee";
    
    public const string Accountant = "Accountant";
    
    public static readonly string[] All = { Admin, Manager, Employee, Accountant };
    
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
            Admin => "Адміністратор системи з повним доступом",
            Manager => "Менеджер проектів з правами управління",
            Employee => "Співробітник з базовими правами",
            Accountant => "Бухгалтер з доступом до фінансових звітів",
            _ => "Користувацька роль"
        };
    }
}