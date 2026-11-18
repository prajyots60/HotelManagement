using HotelManagementService.Models;

namespace HotelManagementService.Repositories.Interfaces;

public interface IDepartmentRepository
{
    Task<List<Department>> GetAllAsync();
    Task<Department?> GetByIdAsync(int id);
    Task<Department> CreateAsync(Department department);
    Task<Department?> UpdateAsync(int id, Department department);
    Task<bool> DeleteAsync(int id);
}
