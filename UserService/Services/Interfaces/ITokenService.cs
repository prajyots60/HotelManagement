using UserService.Models;

namespace UserService.Services.Interfaces;

public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) GenerateToken(User user);
}
