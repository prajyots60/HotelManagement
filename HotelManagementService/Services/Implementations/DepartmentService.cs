using HotelManagementService.DTOs.Departments;
using HotelManagementService.Models;
using HotelManagementService.Repositories.Interfaces;
using HotelManagementService.Services.Interfaces;

namespace HotelManagementService.Services.Implementations;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _repository;

    public DepartmentService(IDepartmentRepository repository)
    {
        _repository = repository;
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

        return updated is null ? null : MapToResponse(updated);
    }

    public Task<bool> DeleteAsync(int id)
    {
        return _repository.DeleteAsync(id);
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
