using FluentValidation;
using HotelManagementService.DTOs.Rooms;

namespace HotelManagementService.Validators.Rooms;

public class UpdateRoomDtoValidator : AbstractValidator<UpdateRoomDto>
{
    private static readonly string[] AllowedStatuses = ["Available", "Occupied", "Maintenance"];

    public UpdateRoomDtoValidator()
    {
        RuleFor(x => x.RoomNumber)
            .NotEmpty().WithMessage("Room number is required.")
            .MaximumLength(20).WithMessage("Room number cannot exceed 20 characters.");

        RuleFor(x => x.RoomType)
            .NotEmpty().WithMessage("Room type is required.")
            .MaximumLength(50).WithMessage("Room type cannot exceed 50 characters.");

        RuleFor(x => x.Capacity)
            .GreaterThanOrEqualTo(1).WithMessage("Capacity must be at least 1.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(status => AllowedStatuses.Any(s => string.Equals(s, status, StringComparison.OrdinalIgnoreCase)))
            .WithMessage("Status must be Available, Occupied, or Maintenance.");
    }
}
