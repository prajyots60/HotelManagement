using HotelManagementService.DTOs.Staff;
using HotelManagementService.Models;
using HotelManagementService.Repositories.Interfaces;
using HotelManagementService.Services.Interfaces;

namespace HotelManagementService.Services.Implementations;

public class StaffService : IStaffService
{
    private readonly IStaffRepository _repository;

    public StaffService(IStaffRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<StaffResponseDto>> GetAllAsync()
    {
        var staff = await _repository.GetAllAsync();
        return staff.Select(MapToResponse).ToList();
    }

    public async Task<StaffResponseDto?> GetByIdAsync(int id)
    {
        var staff = await _repository.GetByIdAsync(id);
        return staff is null ? null : MapToResponse(staff);
    }

    public async Task<(StaffResponseDto? Data, string? Error)> CreateAsync(
        CreateStaffDto dto)
    {
        var staffCode = dto.StaffCode.Trim();
        var nic = dto.NIC.Trim();

        if (!await _repository.DepartmentExistsAsync(dto.DepartmentId))
            return (null, "The specified department does not exist.");

        if (await _repository.GetByCodeAsync(staffCode) is not null)
            return (null, "Staff code already exists.");

        if (await _repository.GetByNicAsync(nic) is not null)
            return (null, "NIC already exists.");

        var staff = new Staff
        {
            StaffCode = staffCode,
            DepartmentId = dto.DepartmentId,
            EmployeeName = dto.EmployeeName.Trim(),
            Address = dto.Address.Trim(),
            NIC = nic,
            Salary = dto.Salary,
            Age = dto.Age,
            Occupation = dto.Occupation.Trim(),
            Email = dto.Email.Trim()
        };

        var created = await _repository.CreateAsync(staff);
        return (MapToResponse(created), null);
    }

    public async Task<(StaffResponseDto? Data, string? Error)> UpdateAsync(
        int id, UpdateStaffDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);

        if (existing is null)
            return (null, null);

        var staffCode = dto.StaffCode.Trim();
        var nic = dto.NIC.Trim();

        if (!await _repository.DepartmentExistsAsync(dto.DepartmentId))
            return (null, "The specified department does not exist.");

        var duplicateCode = await _repository.GetByCodeAsync(staffCode);
        if (duplicateCode is not null && duplicateCode.StaffId != id)
            return (null, "Staff code already exists.");

        var duplicateNic = await _repository.GetByNicAsync(nic);
        if (duplicateNic is not null && duplicateNic.StaffId != id)
            return (null, "NIC already exists.");

        var staff = new Staff
        {
            StaffCode = staffCode,
            DepartmentId = dto.DepartmentId,
            EmployeeName = dto.EmployeeName.Trim(),
            Address = dto.Address.Trim(),
            NIC = nic,
            Salary = dto.Salary,
            Age = dto.Age,
            Occupation = dto.Occupation.Trim(),
            Email = dto.Email.Trim()
        };

        var updated = await _repository.UpdateAsync(id, staff);
        return (updated is null ? null : MapToResponse(updated), null);
    }

    public Task<bool> DeleteAsync(int id)
    {
        return _repository.DeleteAsync(id);
    }

    private static StaffResponseDto MapToResponse(Staff staff)
    {
        return new StaffResponseDto
        {
            StaffId = staff.StaffId,
            StaffCode = staff.StaffCode,
            DepartmentId = staff.DepartmentId,
            DepartmentName = staff.Department?.DepartmentName,
            EmployeeName = staff.EmployeeName,
            Address = staff.Address,
            NIC = staff.NIC,
            Salary = staff.Salary,
            Age = staff.Age,
            Occupation = staff.Occupation,
            Email = staff.Email
        };
    }
}
