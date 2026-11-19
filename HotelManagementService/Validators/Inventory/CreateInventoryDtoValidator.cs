using FluentValidation;
using HotelManagementService.DTOs.Inventory;

namespace HotelManagementService.Validators.Inventory;

public class CreateInventoryDtoValidator : AbstractValidator<CreateInventoryDto>
{
    public CreateInventoryDtoValidator()
    {
        RuleFor(x => x.ItemName)
            .NotEmpty().WithMessage("Item name is required.")
            .MaximumLength(100).WithMessage("Item name cannot exceed 100 characters.");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative.");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("Unit is required.")
            .MaximumLength(30).WithMessage("Unit cannot exceed 30 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(250).WithMessage("Description cannot exceed 250 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
