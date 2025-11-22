namespace TimeTracker.Core.DTOs.Auth;

public class TokenResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; } // В секундах
    public DateTime ExpiresAt { get; set; }
}