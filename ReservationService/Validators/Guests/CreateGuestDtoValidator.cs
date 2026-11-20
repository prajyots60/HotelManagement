using FluentValidation;
using ReservationService.DTOs.Guests;

namespace ReservationService.Validators.Guests;

public class CreateGuestDtoValidator : AbstractValidator<CreateGuestDto>
{
    public CreateGuestDtoValidator()
    {
        RuleFor(x => x.MemberCode)
            .NotEmpty().WithMessage("Member code is required.")
            .MaximumLength(30).WithMessage("Member code cannot exceed 30 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required.")
            .MaximumLength(20).WithMessage("Phone cannot exceed 20 characters.");

        RuleFor(x => x.Company)
            .MaximumLength(100).WithMessage("Company cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters.")
            .EmailAddress().WithMessage("Invalid email address format.");

        RuleFor(x => x.Gender)
            .NotEmpty().WithMessage("Gender is required.")
            .MaximumLength(20).WithMessage("Gender cannot exceed 20 characters.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required.")
            .MaximumLength(250).WithMessage("Address cannot exceed 250 characters.");
    }
}
