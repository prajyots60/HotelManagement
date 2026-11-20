using FluentValidation;
using ReservationService.DTOs.Reservations;

namespace ReservationService.Validators.Reservations;

public class CreateReservationDtoValidator : AbstractValidator<CreateReservationDto>
{
    public CreateReservationDtoValidator()
    {
        RuleFor(x => x.GuestId)
            .GreaterThan(0).WithMessage("GuestId must be greater than zero.");

        RuleFor(x => x.RoomId)
            .GreaterThan(0).WithMessage("RoomId must be greater than zero.");

        RuleFor(x => x.Adults)
            .GreaterThan(0).WithMessage("At least one adult is required.");

        RuleFor(x => x.Children)
            .GreaterThanOrEqualTo(0).WithMessage("Children cannot be negative.");

        RuleFor(x => x.CheckIn)
            .NotEmpty().WithMessage("Check-in date is required.");

        RuleFor(x => x.CheckOut)
            .NotEmpty().WithMessage("Check-out date is required.")
            .GreaterThan(x => x.CheckIn).WithMessage("Check-out date must be later than check-in date.");
    }
}
