namespace TimeTracker.Core.Common;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";
    
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
    public int RefreshTokenExpirationDays { get; set; } = 7;
    public int ClockSkew { get; set; } = 5;
}