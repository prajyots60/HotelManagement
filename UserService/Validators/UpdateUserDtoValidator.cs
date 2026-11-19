using FluentValidation;
using UserService.DTOs.Users;

namespace UserService.Validators;

public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required.")
            .MinimumLength(3)
            .WithMessage("Username must contain at least 3 characters.")
            .MaximumLength(50)
            .WithMessage("Username cannot exceed 50 characters.");

        RuleFor(x => x.Role)
            .NotEmpty()
            .WithMessage("Role is required.")
            .Must(role => new[] { "Owner", "Manager", "Receptionist" }
                .Contains(role))
            .WithMessage("Role must be Owner, Manager, or Receptionist.");
    }
}
