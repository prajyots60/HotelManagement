using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

        // Ensure database is created and all pending migrations are applied
        await context.Database.MigrateAsync();

        // Seed initial Owner account if no Owner exists
        if (!await context.Users.AnyAsync(u => u.Role == "Owner"))
        {
            var initialOwner = new User
            {
                Username = "admin_owner",
                Role = "Owner"
            };

            initialOwner.PasswordHash = passwordHasher.HashPassword(initialOwner, "Owner@12345");

            await context.Users.AddAsync(initialOwner);
            await context.SaveChangesAsync();
        }
    }
}
