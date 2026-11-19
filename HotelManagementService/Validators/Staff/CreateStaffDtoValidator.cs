using FluentValidation;
using HotelManagementService.DTOs.Staff;

namespace HotelManagementService.Validators.Staff;

public class CreateStaffDtoValidator : AbstractValidator<CreateStaffDto>
{
    public CreateStaffDtoValidator()
    {
        RuleFor(x => x.StaffCode)
            .NotEmpty().WithMessage("Staff code is required.")
            .MaximumLength(30).WithMessage("Staff code cannot exceed 30 characters.");

        RuleFor(x => x.DepartmentId)
            .GreaterThan(0).WithMessage("Valid DepartmentId is required.");

        RuleFor(x => x.EmployeeName)
            .NotEmpty().WithMessage("Employee name is required.")
            .MaximumLength(100).WithMessage("Employee name cannot exceed 100 characters.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required.")
            .MaximumLength(250).WithMessage("Address cannot exceed 250 characters.");

        RuleFor(x => x.NIC)
            .NotEmpty().WithMessage("NIC is required.")
            .MaximumLength(20).WithMessage("NIC cannot exceed 20 characters.");

        RuleFor(x => x.Salary)
            .InclusiveBetween(0m, 99999999.99m).WithMessage("Salary must be between 0 and 99,999,999.99.");

        RuleFor(x => x.Age)
            .InclusiveBetween(18, 100).WithMessage("Age must be between 18 and 100.");

        RuleFor(x => x.Occupation)
            .NotEmpty().WithMessage("Occupation is required.")
            .MaximumLength(80).WithMessage("Occupation cannot exceed 80 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters.");
    }
}
