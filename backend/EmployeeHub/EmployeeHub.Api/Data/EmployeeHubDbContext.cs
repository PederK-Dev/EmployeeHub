using EmployeeHub.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeHub.Api.Data;

public class EmployeeHubDbContext : DbContext
{
    public EmployeeHubDbContext(DbContextOptions<EmployeeHubDbContext> options)
        : base(options)
    {
    }

    public DbSet<Department> Departments { get; set; }

    public DbSet<Position> Positions { get; set; }

    public DbSet<Employee> Employees { get; set; }

    public DbSet<LeaveRequest> LeaveRequests { get; set; }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>(entity =>
        {
            entity.Property(d => d.Name).IsRequired().HasMaxLength(100);
            entity.Property(d => d.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<Position>(entity =>
        {
            entity.Property(p => p.Title).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.HasIndex(e => e.Email).IsUnique();

            entity.HasOne(e => e.Department)
                .WithMany()
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Position)
                .WithMany(p => p.Employees)
                .HasForeignKey(e => e.PositionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.Property(l => l.Type)
                .HasConversion<string>()
                .HasMaxLength(50);
            entity.Property(l => l.Status)
                .HasConversion<string>()
                .HasMaxLength(50);
            entity.Property(l => l.Reason).HasMaxLength(500);

            entity.HasOne(l => l.Employee)
                .WithMany(e => e.LeaveRequests)
                .HasForeignKey(l => l.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Role)
                .HasConversion<string>()
                .HasMaxLength(50);
            entity.Property(u => u.EmailVerificationToken).HasMaxLength(128);
            entity.Property(u => u.PasswordResetToken).HasMaxLength(128);

            entity.HasOne(u => u.Employee)
                .WithOne()
                .HasForeignKey<User>(u => u.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
