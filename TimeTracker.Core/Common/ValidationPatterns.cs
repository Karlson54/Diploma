namespace TimeTracker.Core.Common;

public static class ValidationPatterns
{
    public const int PasswordMinLength = 8;
    
    public const int PasswordMaxLength = 100;
    
    public const string PasswordSpecialChars = "!@#$%^&*()_+\\-=\\[\\]{}|;:,.<>?/~`";
    
    public const string Password = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{}|;:,.<>?/~`]).{8,}$";
    
    public const string PasswordErrorMessage = 
        "Password має містити мінімум 8 символів, хоча б одну велику літеру, одну малу літеру, одну цифру та один спеціальний символ";

    public const int LoginMinLength = 3;
    
    public const int LoginMaxLength = 50;
    
    public const string Login = @"^[a-zA-Z][a-zA-Z0-9_]{2,49}$";
    
    public const string LoginErrorMessage = 
        "Login має починатися з літери та містити тільки літери, цифри та підкреслення (3-50 символів)";

    public const int EmailMaxLength = 100;
    
    public const string Email = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
    
    public const string EmailErrorMessage = "Некоректний формат email адреси";

    public const int NameMinLength = 2;
    
    public const int NameMaxLength = 100;

    public const int RoleNameMinLength = 3;
    
    public const int RoleNameMaxLength = 50;
    
    public const string RoleName = @"^[a-zA-Z][a-zA-Z0-9_]{2,49}$";
    
    public const string RoleNameErrorMessage = 
        "Назва ролі має починатися з літери та містити тільки літери, цифри та підкреслення (3-50 символів)";

    public const int DescriptionMaxLength = 500;

    public const int PermissionsMaxLength = 4000;
    
    public const int MaxPermissionsCount = 100;
    
    public const int MaxPermissionLength = 200;

    public const int MaxUserRolesCount = 10;
}