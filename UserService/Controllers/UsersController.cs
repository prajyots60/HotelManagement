using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Authorization;
using UserService.DTOs.Users;
using UserService.Services.Interfaces;

namespace UserService.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Policy = AuthorizationPolicies.UsersRead)]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IValidator<LoginDto> _loginValidator;
    private readonly IValidator<CreateUserDto> _createUserValidator;
    private readonly IValidator<UpdateUserDto> _updateUserValidator;

    public UsersController(
        IUserService userService,
        IValidator<LoginDto> loginValidator,
        IValidator<CreateUserDto> createUserValidator,
        IValidator<UpdateUserDto> updateUserValidator)
    {
        _userService = userService;
        _loginValidator = loginValidator;
        _createUserValidator = createUserValidator;
        _updateUserValidator = updateUserValidator;
    }

    // POST: api/users/login
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var validationResult = await _loginValidator.ValidateAsync(dto);

        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }

            return ValidationProblem(ModelState);
        }

        var user = await _userService.LoginAsync(dto);

        if (user is null)
        {
            return Unauthorized(new
            {
                message = "Invalid username or password."
            });
        }

        return Ok(user);
    }

    // GET: api/users
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    // GET: api/users/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid user ID." });
        }

        var user = await _userService.GetUserByIdAsync(id);

        if (user is null)
        {
            return NotFound(new { message = "User not found." });
        }

        return Ok(user);
    }

    // POST: api/users
    [Authorize(Policy = AuthorizationPolicies.UsersManage)]
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        var validationResult = await _createUserValidator.ValidateAsync(dto);

        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }

            return ValidationProblem(ModelState);
        }

        var user = await _userService.CreateUserAsync(dto);

        return CreatedAtAction(
            nameof(GetUserById),
            new { id = user.UserId },
            user);
    }

    // PUT: api/users/5
    [Authorize(Policy = AuthorizationPolicies.UsersManage)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateUser(
        int id,
        [FromBody] UpdateUserDto dto)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid user ID." });
        }

        var validationResult = await _updateUserValidator.ValidateAsync(dto);

        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }

            return ValidationProblem(ModelState);
        }

        var user = await _userService.UpdateUserAsync(id, dto);

        if (user is null)
        {
            return NotFound(new { message = "User not found." });
        }

        return Ok(user);
    }

    // DELETE: api/users/5
    [Authorize(Policy = AuthorizationPolicies.UsersDelete)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid user ID." });
        }

        var deleted = await _userService.DeleteUserAsync(id);

        if (!deleted)
        {
            return NotFound(new { message = "User not found." });
        }

        return NoContent();
    }
}
