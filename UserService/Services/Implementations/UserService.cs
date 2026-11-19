using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using UserService.DTOs.Users;
using UserService.Exceptions;
using UserService.Models;
using UserService.Repositories.Interfaces;
using UserService.Services.Interfaces;

namespace UserService.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ILogger<UserService> _logger;
    private readonly ICurrentUserService _currentUserService;

    public UserService(
        IUserRepository userRepository,
        IPasswordHasher<User> passwordHasher,
        ITokenService tokenService,
        ILogger<UserService> logger,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _logger = logger;
        _currentUserService = currentUserService;
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

        ValidateRoleAssignment(dto.Role);

        if (await _userRepository.UsernameExistsAsync(username))
        {
            throw new ConflictException("Username already exists.");
        }

        var user = new User
        {
            Username = username,
            Role = dto.Role,
            PasswordHash = string.Empty
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

        var createdUser = await _userRepository.AddAsync(user);

        _logger.LogInformation(
            "User created successfully. UserId: {UserId}, Role: {Role}",
            createdUser.UserId,
            createdUser.Role);

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

        ValidateRoleAssignment(dto.Role);

        var username = dto.Username.Trim();

        if (!string.Equals(user.Username, username, StringComparison.OrdinalIgnoreCase)
            && await _userRepository.UsernameExistsAsync(username))
        {
            throw new ConflictException("Username already exists.");
        }

        user.Username = username;
        user.Role = dto.Role;

        await _userRepository.UpdateAsync(user);

        _logger.LogInformation(
            "User updated successfully. UserId: {UserId}",
            user.UserId);

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

        _logger.LogInformation(
            "User deleted successfully. UserId: {UserId}",
            user.UserId);

        return true;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
    {
        var username = dto.Username.Trim();
        var user = await _userRepository.GetByUsernameAsync(username);

        if (user is null)
        {
            _logger.LogWarning("Authentication failed due to invalid credentials.");
            return null;
        }

        var result = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            dto.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            _logger.LogWarning("Authentication failed due to invalid credentials.");
            return null;
        }

        var tokenResult = _tokenService.GenerateToken(user);

        _logger.LogInformation(
            "User authenticated successfully. UserId: {UserId}",
            user.UserId);

        return new LoginResponseDto
        {
            Token = tokenResult.Token,
            ExpiresAtUtc = tokenResult.ExpiresAtUtc,
            User = MapToResponse(user)
        };
    }

    private void ValidateRoleAssignment(string requestedRole)
    {
        var currentRole = _currentUserService.Role;

        if (currentRole == "Owner")
        {
            return;
        }

        if (currentRole == "Manager" && requestedRole != "Owner")
        {
            return;
        }

        throw new ForbiddenException(
            "You are not permitted to assign the requested role.");
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
