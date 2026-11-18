using HotelManagementService.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementService.Data;

public class HotelDbContext : DbContext
{
    public HotelDbContext(DbContextOptions<HotelDbContext> options)
        : base(options)
    {
    }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<RoomRate> RoomRates => Set<RoomRate>();
    public DbSet<InventoryItem> Inventory => Set<InventoryItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Department>()
            .HasKey(d => d.DepartmentId);

        modelBuilder.Entity<Staff>()
            .HasKey(s => s.StaffId);

        modelBuilder.Entity<Staff>()
            .HasIndex(s => s.StaffCode)
            .IsUnique();

        modelBuilder.Entity<Staff>()
            .HasIndex(s => s.NIC)
            .IsUnique();

        modelBuilder.Entity<Staff>()
            .Property(s => s.Salary)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Staff>()
            .HasOne(s => s.Department)
            .WithMany(d => d.StaffMembers)
            .HasForeignKey(s => s.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Room>()
            .HasKey(r => r.RoomId);

        modelBuilder.Entity<Room>()
            .HasIndex(r => r.RoomNumber)
            .IsUnique();

        modelBuilder.Entity<Room>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_Rooms_Capacity",
                "[Capacity] > 0"));

        modelBuilder.Entity<RoomRate>()
            .HasKey(r => r.RateId);

        modelBuilder.Entity<RoomRate>()
            .Property(r => r.FirstNightPrice)
            .HasPrecision(10, 2);

        modelBuilder.Entity<RoomRate>()
            .Property(r => r.ExtensionPrice)
            .HasPrecision(10, 2);

        modelBuilder.Entity<RoomRate>()
            .HasOne(r => r.Room)
            .WithMany(room => room.RoomRates)
            .HasForeignKey(r => r.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<InventoryItem>()
            .HasKey(i => i.InventoryId);

        modelBuilder.Entity<InventoryItem>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_Inventory_Quantity",
                "[Quantity] >= 0"));
    }
}
