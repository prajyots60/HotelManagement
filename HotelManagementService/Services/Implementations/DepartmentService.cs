using HotelManagementService.DTOs.Departments;
using HotelManagementService.Models;
using HotelManagementService.Repositories.Interfaces;
using HotelManagementService.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace HotelManagementService.Services.Implementations;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _repository;
    private readonly ILogger<DepartmentService> _logger;

    public DepartmentService(
        IDepartmentRepository repository,
        ILogger<DepartmentService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<DepartmentResponseDto>> GetAllAsync()
    {
        var departments = await _repository.GetAllAsync();

        return departments.Select(MapToResponse).ToList();
    }

    public async Task<DepartmentResponseDto?> GetByIdAsync(int id)
    {
        var department = await _repository.GetByIdAsync(id);

        return department is null ? null : MapToResponse(department);
    }

    public async Task<DepartmentResponseDto> CreateAsync(
        CreateDepartmentDto dto)
    {
        var department = new Department
        {
            DepartmentName = dto.DepartmentName.Trim(),
            Description = dto.Description?.Trim()
        };

        var created = await _repository.CreateAsync(department);

        _logger.LogInformation(
            "Department created successfully with DepartmentId {DepartmentId} and DepartmentName {DepartmentName}",
            created.DepartmentId,
            created.DepartmentName);

        return MapToResponse(created);
    }

    public async Task<DepartmentResponseDto?> UpdateAsync(
        int id, UpdateDepartmentDto dto)
    {
        var department = new Department
        {
            DepartmentName = dto.DepartmentName.Trim(),
            Description = dto.Description?.Trim()
        };

        var updated = await _repository.UpdateAsync(id, department);

        if (updated is null)
        {
            _logger.LogWarning(
                "Failed to update department. Department with DepartmentId {DepartmentId} was not found",
                id);
            return null;
        }

        _logger.LogInformation(
            "Department {DepartmentId} updated successfully to DepartmentName {DepartmentName}",
            updated.DepartmentId,
            updated.DepartmentName);

        return MapToResponse(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var deleted = await _repository.DeleteAsync(id);

        if (!deleted)
        {
            _logger.LogWarning(
                "Failed to delete department. Department with DepartmentId {DepartmentId} was not found",
                id);
            return false;
        }

        _logger.LogInformation(
            "Department {DepartmentId} deleted successfully",
            id);

        return true;
    }

    private static DepartmentResponseDto MapToResponse(
        Department department)
    {
        return new DepartmentResponseDto
        {
            DepartmentId = department.DepartmentId,
            DepartmentName = department.DepartmentName,
            Description = department.Description
        };
    }
}
