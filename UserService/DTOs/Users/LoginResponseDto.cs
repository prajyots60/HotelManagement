namespace UserService.DTOs.Users;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public UserResponseDto User { get; set; } = new();
}
