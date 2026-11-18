using System.ComponentModel.DataAnnotations;

namespace UserService.DTOs.Users;

public class UpdateUserDto
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [RegularExpression("^(Owner|Manager|Receptionist)$")]
    public string Role { get; set; } = string.Empty;
}
