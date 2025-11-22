namespace TimeTracker.Core.DTOs.Auth;

public class AuthResponseDto
{
    public long UserId { get; set; }
    public string Login { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long AgencyId { get; set; }
    public string AgencyName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}