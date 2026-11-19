namespace ReservationService.Configuration;

/// <summary>
/// Strongly-typed configuration options for JWT validation.
/// Binds to the "Jwt" section from User Secrets and appsettings.
/// </summary>
public class JwtSettings
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public int ExpiresInMinutes { get; set; } = 60;
}
