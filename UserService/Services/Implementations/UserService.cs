using Microsoft.AspNetCore.Identity;
using UserService.DTOs.Users;
using UserService.Models;
using UserService.Repositories.Interfaces;
using UserService.Services.Interfaces;

namespace UserService.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ITokenService _tokenService;

    public UserService(
        IUserRepository userRepository,
        IPasswordHasher<User> passwordHasher,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();

        return users.Select(MapToResponse);
    }

    public async Task<UserResponseDto?> GetUserByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);

        return user is null ? null : MapToResponse(user);
    }

    public async Task<UserResponseDto> CreateUserAsync(CreateUserDto dto)
    {
        var username = dto.Username.Trim();

        if (await _userRepository.UsernameExistsAsync(username))
        {
            throw new InvalidOperationException("Username already exists.");
        }

        var user = new User
        {
            Username = username,
            Role = dto.Role,
            PasswordHash = string.Empty
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

        var createdUser = await _userRepository.AddAsync(user);

        return MapToResponse(createdUser);
    }

    public async Task<UserResponseDto?> UpdateUserAsync(
        int id,
        UpdateUserDto dto)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user is null)
        {
            return null;
        }

        var username = dto.Username.Trim();

        if (!string.Equals(user.Username, username, StringComparison.OrdinalIgnoreCase)
            && await _userRepository.UsernameExistsAsync(username))
        {
            throw new InvalidOperationException("Username already exists.");
        }

        user.Username = username;
        user.Role = dto.Role;

        await _userRepository.UpdateAsync(user);

        return MapToResponse(user);
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user is null)
        {
            return false;
        }

        await _userRepository.DeleteAsync(user);

        return true;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
    {
        var username = dto.Username.Trim();
        var user = await _userRepository.GetByUsernameAsync(username);

        if (user is null)
        {
            return null;
        }

        var result = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            dto.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }

        var tokenResult = _tokenService.GenerateToken(user);

        return new LoginResponseDto
        {
            Token = tokenResult.Token,
            ExpiresAtUtc = tokenResult.ExpiresAtUtc,
            User = MapToResponse(user)
        };
    }

    private static UserResponseDto MapToResponse(User user)
    {
        return new UserResponseDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Role = user.Role
        };
    }
}
