using FluentValidation;
using ReservationService.DTOs.Reservations;

namespace ReservationService.Validators.Reservations;

public class UpdateReservationStatusDtoValidator : AbstractValidator<UpdateReservationStatusDto>
{
    private static readonly string[] AllowedStatuses =
    [
        "Pending",
        "Confirmed",
        "Cancelled",
        "CheckedIn",
        "CheckedOut"
    ];

    public UpdateReservationStatusDtoValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(status => AllowedStatuses.Contains(status))
            .WithMessage("Invalid reservation status. Allowed values: Pending, Confirmed, Cancelled, CheckedIn, CheckedOut.");
    }
}
