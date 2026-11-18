using HotelManagementService.Models;

namespace HotelManagementService.Repositories.Interfaces;

public interface IStaffRepository
{
    Task<List<Staff>> GetAllAsync();
    Task<Staff?> GetByIdAsync(int id);
    Task<Staff?> GetByCodeAsync(string staffCode);
    Task<Staff?> GetByNicAsync(string nic);
    Task<bool> DepartmentExistsAsync(int departmentId);
    Task<Staff> CreateAsync(Staff staff);
    Task<Staff?> UpdateAsync(int id, Staff staff);
    Task<bool> DeleteAsync(int id);
}
