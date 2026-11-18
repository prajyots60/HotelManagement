using HotelManagementService.Data;
using HotelManagementService.Models;
using HotelManagementService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementService.Repositories.Implementations;

public class StaffRepository : IStaffRepository
{
    private readonly HotelDbContext _context;

    public StaffRepository(HotelDbContext context)
    {
        _context = context;
    }

    public Task<List<Staff>> GetAllAsync()
    {
        return _context.Staff
            .AsNoTracking()
            .Include(s => s.Department)
            .ToListAsync();
    }

    public Task<Staff?> GetByIdAsync(int id)
    {
        return _context.Staff
            .AsNoTracking()
            .Include(s => s.Department)
            .FirstOrDefaultAsync(s => s.StaffId == id);
    }

    public Task<Staff?> GetByCodeAsync(string staffCode)
    {
        return _context.Staff
            .FirstOrDefaultAsync(s => s.StaffCode == staffCode);
    }

    public Task<Staff?> GetByNicAsync(string nic)
    {
        return _context.Staff
            .FirstOrDefaultAsync(s => s.NIC == nic);
    }

    public Task<bool> DepartmentExistsAsync(int departmentId)
    {
        return _context.Departments
            .AnyAsync(d => d.DepartmentId == departmentId);
    }

    public async Task<Staff> CreateAsync(Staff staff)
    {
        _context.Staff.Add(staff);
        await _context.SaveChangesAsync();

        return await _context.Staff
            .AsNoTracking()
            .Include(s => s.Department)
            .FirstAsync(s => s.StaffId == staff.StaffId);
    }

    public async Task<Staff?> UpdateAsync(int id, Staff staff)
    {
        var existing = await _context.Staff.FindAsync(id);

        if (existing is null)
            return null;

        existing.StaffCode = staff.StaffCode;
        existing.DepartmentId = staff.DepartmentId;
        existing.EmployeeName = staff.EmployeeName;
        existing.Address = staff.Address;
        existing.NIC = staff.NIC;
        existing.Salary = staff.Salary;
        existing.Age = staff.Age;
        existing.Occupation = staff.Occupation;
        existing.Email = staff.Email;

        await _context.SaveChangesAsync();

        return await _context.Staff
            .AsNoTracking()
            .Include(s => s.Department)
            .FirstAsync(s => s.StaffId == id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var staff = await _context.Staff.FindAsync(id);

        if (staff is null)
            return false;

        _context.Staff.Remove(staff);
        await _context.SaveChangesAsync();

        return true;
    }
}
