using FluentValidation;
using UserService.DTOs.Users;

namespace UserService.Validators;

public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required.")
            .MinimumLength(3)
            .WithMessage("Username must contain at least 3 characters.")
            .MaximumLength(50)
            .WithMessage("Username cannot exceed 50 characters.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MinimumLength(8)
            .WithMessage("Password must contain at least 8 characters.")
            .MaximumLength(100)
            .WithMessage("Password cannot exceed 100 characters.");

        RuleFor(x => x.Role)
            .Must(role => new[] { "Owner", "Manager", "Receptionist" }
                .Contains(role))
            .WithMessage("Role must be Owner, Manager, or Receptionist.");
    }
}
