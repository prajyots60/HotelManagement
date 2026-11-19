using FluentValidation;
using HotelManagementService.DTOs.RoomRates;

namespace HotelManagementService.Validators.RoomRates;

public class UpdateRoomRateDtoValidator : AbstractValidator<UpdateRoomRateDto>
{
    public UpdateRoomRateDtoValidator()
    {
        RuleFor(x => x.RoomId)
            .GreaterThan(0).WithMessage("Valid RoomId is required.");

        RuleFor(x => x.StartDate)
            .Must(d => d != default).WithMessage("Start date is required.");

        RuleFor(x => x.EndDate)
            .Must(d => d != default).WithMessage("End date is required.")
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("End date must be greater than or equal to start date.");

        RuleFor(x => x.Day)
            .NotEmpty().WithMessage("Day is required.")
            .MaximumLength(15).WithMessage("Day cannot exceed 15 characters.");

        RuleFor(x => x.GuestCount)
            .GreaterThanOrEqualTo(1).WithMessage("Guest count must be at least 1.");

        RuleFor(x => x.FirstNightPrice)
            .InclusiveBetween(0.01m, 99999999.99m).WithMessage("First night price must be between 0.01 and 99,999,999.99.");

        RuleFor(x => x.ExtensionPrice)
            .InclusiveBetween(0.01m, 99999999.99m).WithMessage("Extension price must be between 0.01 and 99,999,999.99.");
    }
}
