namespace TimeTracker.Core.DTOs.Auth;

public class AuthResult
{
    public bool IsSuccess { get; private set; }
    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }
    public long? UserId { get; private set; }
    public string? UserName { get; private set; }
    public string? UserEmail { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    private AuthResult() { }

    public static AuthResult Success(string accessToken, string refreshToken, 
        long userId, string userName, string userEmail, DateTime? expiresAt = null)
    {
        return new AuthResult
        {
            IsSuccess = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            UserId = userId,
            UserName = userName,
            UserEmail = userEmail,
            ExpiresAt = expiresAt ?? DateTime.UtcNow.AddHours(1)
        };
    }

    public static AuthResult Success(string message)
    {
        return new AuthResult { IsSuccess = true };
    }

    public static AuthResult Failure(string errorMessage)
    {
        return new AuthResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}