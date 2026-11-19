using FluentValidation;
using HotelManagementService.DTOs.Departments;

namespace HotelManagementService.Validators.Departments;

public class UpdateDepartmentDtoValidator : AbstractValidator<UpdateDepartmentDto>
{
    public UpdateDepartmentDtoValidator()
    {
        RuleFor(x => x.DepartmentName)
            .NotEmpty().WithMessage("Department name is required.")
            .MaximumLength(100).WithMessage("Department name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(250).WithMessage("Description cannot exceed 250 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
