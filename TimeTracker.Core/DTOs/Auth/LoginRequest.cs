namespace TimeTracker.Core.DTOs.Auth;

public class LoginRequest
{
    public string EmailOrLogin { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
