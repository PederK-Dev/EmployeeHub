using EmployeeHub.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EmployeeHub.Api.Data;

public static class DbSeeder
{
    /// <summary>
    /// Applies pending migrations and seeds a starter admin user plus a small amount
    /// of demo data so the UI has something to show. All steps are idempotent.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EmployeeHubDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        await context.Database.MigrateAsync();

        if (!await context.Departments.AnyAsync())
        {
            context.Departments.AddRange(
                new Department { Name = "Engineering", Description = "Builds and maintains the product." },
                new Department { Name = "Human Resources", Description = "People operations and hiring." });
        }

        if (!await context.Positions.AnyAsync())
        {
            context.Positions.AddRange(
                new Position { Title = "Software Engineer", Description = "Designs and writes software." },
                new Position { Title = "HR Manager", Description = "Leads HR operations." });
        }

        await context.SaveChangesAsync();

        if (!await context.Users.AnyAsync())
        {
            var adminEmail = configuration["Seed:AdminEmail"] ?? "admin@employeehub.local";
            var adminPassword = configuration["Seed:AdminPassword"] ?? "Admin123!";

            var admin = new User
            {
                Email = adminEmail,
                Role = UserRole.Admin
            };
            admin.PasswordHash = passwordHasher.HashPassword(admin, adminPassword);

            context.Users.Add(admin);
            await context.SaveChangesAsync();
        }
    }
}
