using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.DTOs.Users;
using UserService.Services.Interfaces;

namespace UserService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    // POST: api/users/login
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
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
    [Authorize(Roles = "Owner,Manager")]
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        try
        {
            var user = await _userService.CreateUserAsync(dto);

            return CreatedAtAction(
                nameof(GetUserById),
                new { id = user.UserId },
                user);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // PUT: api/users/5
    [Authorize(Roles = "Owner,Manager")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateUser(
        int id,
        [FromBody] UpdateUserDto dto)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid user ID." });
        }

        try
        {
            var user = await _userService.UpdateUserAsync(id, dto);

            if (user is null)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // DELETE: api/users/5
    [Authorize(Roles = "Owner")]
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
