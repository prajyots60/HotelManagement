using System.Security.Claims;
using UserService.Services.Interfaces;

namespace UserService.Infrastructure.Security;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? Role =>
        _httpContextAccessor.HttpContext?
            .User.FindFirstValue(ClaimTypes.Role);
}
