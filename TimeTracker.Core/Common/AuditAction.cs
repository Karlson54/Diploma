namespace TimeTracker.Core.Common;

public static class AuditAction
{
    // CRUD Operations
    public const string Create = "Create";
    public const string Read = "Read";
    public const string Update = "Update";
    public const string Delete = "Delete";
    
    // Authentication
    public const string Login = "Login";
    public const string Logout = "Logout";
    public const string LoginFailed = "LoginFailed";
    public const string PasswordChanged = "PasswordChanged";
    
    // Authorization
    public const string RoleAssigned = "RoleAssigned";
    public const string RoleRemoved = "RoleRemoved";
    public const string PermissionsChanged = "PermissionsChanged";
    
    // User Management
    public const string UserActivated = "UserActivated";
    public const string UserDeactivated = "UserDeactivated";
    
    // Dictionary Management
    public const string DictionaryActivated = "DictionaryActivated";
    public const string DictionaryDeactivated = "DictionaryDeactivated";
    
    // Bulk Operations
    public const string BulkCreate = "BulkCreate";
    public const string BulkUpdate = "BulkUpdate";
    public const string BulkDelete = "BulkDelete";
}