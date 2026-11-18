using System.ComponentModel.DataAnnotations;

namespace UserService.DTOs.Users;

public class CreateUserDto
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(Owner|Manager|Receptionist)$")]
    public string Role { get; set; } = string.Empty;
}
