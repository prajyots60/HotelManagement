using HotelManagementService.DTOs.Staff;
using HotelManagementService.Exceptions;
using HotelManagementService.Models;
using HotelManagementService.Repositories.Interfaces;
using HotelManagementService.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace HotelManagementService.Services.Implementations;

public class StaffService : IStaffService
{
    private readonly IStaffRepository _repository;
    private readonly ILogger<StaffService> _logger;

    public StaffService(
        IStaffRepository repository,
        ILogger<StaffService> logger)
    {
        _repository = repository;
        _logger = logger;
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

    public async Task<StaffResponseDto> CreateAsync(CreateStaffDto dto)
    {
        var staffCode = dto.StaffCode.Trim();
        var nic = dto.NIC.Trim();

        if (!await _repository.DepartmentExistsAsync(dto.DepartmentId))
        {
            _logger.LogWarning(
                "Failed to create staff. Department with DepartmentId {DepartmentId} does not exist",
                dto.DepartmentId);
            throw new NotFoundException("The specified department does not exist.");
        }

        if (await _repository.GetByCodeAsync(staffCode) is not null)
        {
            _logger.LogWarning(
                "Failed to create staff. StaffCode {StaffCode} already exists",
                staffCode);
            throw new ConflictException("Staff code already exists.");
        }

        if (await _repository.GetByNicAsync(nic) is not null)
        {
            _logger.LogWarning(
                "Failed to create staff. A staff member with the provided NIC already exists");
            throw new ConflictException("NIC already exists.");
        }

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

        _logger.LogInformation(
            "Staff member created successfully with StaffId {StaffId} and StaffCode {StaffCode}",
            created.StaffId,
            created.StaffCode);

        return MapToResponse(created);
    }

    public async Task<StaffResponseDto?> UpdateAsync(int id, UpdateStaffDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);

        if (existing is null)
        {
            _logger.LogWarning(
                "Failed to update staff. Staff member with StaffId {StaffId} was not found",
                id);
            return null;
        }

        var staffCode = dto.StaffCode.Trim();
        var nic = dto.NIC.Trim();

        if (!await _repository.DepartmentExistsAsync(dto.DepartmentId))
        {
            _logger.LogWarning(
                "Failed to update staff {StaffId}. Department with DepartmentId {DepartmentId} does not exist",
                id,
                dto.DepartmentId);
            throw new NotFoundException("The specified department does not exist.");
        }

        var duplicateCode = await _repository.GetByCodeAsync(staffCode);
        if (duplicateCode is not null && duplicateCode.StaffId != id)
        {
            _logger.LogWarning(
                "Failed to update staff {StaffId}. StaffCode {StaffCode} already belongs to another staff member",
                id,
                staffCode);
            throw new ConflictException("Staff code already exists.");
        }

        var duplicateNic = await _repository.GetByNicAsync(nic);
        if (duplicateNic is not null && duplicateNic.StaffId != id)
        {
            _logger.LogWarning(
                "Failed to update staff {StaffId}. The provided NIC already belongs to another staff member",
                id);
            throw new ConflictException("NIC already exists.");
        }

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
        if (updated is null)
        {
            _logger.LogWarning(
                "Failed to update staff. Staff member with StaffId {StaffId} was not found during persist",
                id);
            return null;
        }

        _logger.LogInformation(
            "Staff member {StaffId} updated successfully with StaffCode {StaffCode}",
            updated.StaffId,
            updated.StaffCode);

        return MapToResponse(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var deleted = await _repository.DeleteAsync(id);

        if (!deleted)
        {
            _logger.LogWarning(
                "Failed to delete staff. Staff member with StaffId {StaffId} was not found",
                id);
            return false;
        }

        _logger.LogInformation(
            "Staff member {StaffId} deleted successfully",
            id);

        return true;
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
