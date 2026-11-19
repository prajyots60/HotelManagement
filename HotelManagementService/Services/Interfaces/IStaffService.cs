using HotelManagementService.DTOs.Staff;

namespace HotelManagementService.Services.Interfaces;

public interface IStaffService
{
    Task<List<StaffResponseDto>> GetAllAsync();
    Task<StaffResponseDto?> GetByIdAsync(int id);
    Task<StaffResponseDto> CreateAsync(CreateStaffDto dto);
    Task<StaffResponseDto?> UpdateAsync(int id, UpdateStaffDto dto);
    Task<bool> DeleteAsync(int id);
}
