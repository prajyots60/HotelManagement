using Microsoft.EntityFrameworkCore;
using ReservationService.Models;

namespace ReservationService.Data;

public class ReservationDbContext : DbContext
{
    public ReservationDbContext(
        DbContextOptions<ReservationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Guest> Guests => Set<Guest>();

    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Guest configuration
        modelBuilder.Entity<Guest>(entity =>
        {
            entity.HasKey(g => g.GuestId);

            entity.HasIndex(g => g.MemberCode)
                .IsUnique();

            entity.Property(g => g.MemberCode)
                .HasMaxLength(30)
                .IsRequired();
        });

        // Reservation configuration
        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(r => r.ReservationId);

            entity.HasIndex(r => r.ReservationCode)
                .IsUnique();

            entity.Property(r => r.ReservationCode)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(r => r.Status)
                .HasMaxLength(20)
                .IsRequired();

            entity.HasOne(r => r.Guest)
                .WithMany(g => g.Reservations)
                .HasForeignKey(r => r.GuestId)
                .OnDelete(DeleteBehavior.Restrict);

            // RoomId is deliberately NOT configured as a database FK.
        });
    }
}
