using HotelManagementService.Data;
using HotelManagementService.Models;
using HotelManagementService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementService.Repositories.Implementations;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly HotelDbContext _context;

    public DepartmentRepository(HotelDbContext context)
    {
        _context = context;
    }

    public async Task<List<Department>> GetAllAsync()
    {
        return await _context.Departments
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Department?> GetByIdAsync(int id)
    {
        return await _context.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DepartmentId == id);
    }

    public async Task<Department> CreateAsync(Department department)
    {
        _context.Departments.Add(department);
        await _context.SaveChangesAsync();

        return department;
    }

    public async Task<Department?> UpdateAsync(
        int id, Department department)
    {
        var existing = await _context.Departments.FindAsync(id);

        if (existing is null)
            return null;

        existing.DepartmentName = department.DepartmentName;
        existing.Description = department.Description;

        await _context.SaveChangesAsync();

        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var department = await _context.Departments.FindAsync(id);

        if (department is null)
            return false;

        _context.Departments.Remove(department);
        await _context.SaveChangesAsync();

        return true;
    }
}
