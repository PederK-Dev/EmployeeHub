using System.Data.Common;
using EmployeeHub.Api.Data;
using EmployeeHub.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EmployeeHub.Api.Tests;

/// <summary>
/// Boots the real API pipeline — the same authorization policies, attributes and services that run
/// in production — against a throwaway SQLite database. Testing the assembled pipeline rather than
/// services in isolation is deliberate: most of the access control lives in <c>[Authorize]</c>
/// attributes, which service-level tests would never exercise.
/// </summary>
public class EmployeeHubApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string Password = "Str0ngPassword!";

    private DbConnection _connection = null!;

    /// <summary>Ada's employee row; the <see cref="EmployeeUser"/> account is linked to it.</summary>
    public int AdaEmployeeId { get; private set; }

    /// <summary>Grace's employee row, deliberately belonging to nobody's account.</summary>
    public int GraceEmployeeId { get; private set; }

    /// <summary>Grace's leave request — the one Ada must not be able to see or touch.</summary>
    public int GraceLeaveRequestId { get; private set; }

    public string AdminUser => "admin@test.local";

    public string ManagerUser => "manager@test.local";

    /// <summary>Employee-role account linked to Ada.</summary>
    public string EmployeeUser => "ada@test.local";

    /// <summary>Employee-role account with no employee record behind it, as self-registration creates.</summary>
    public string UnlinkedUser => "unlinked@test.local";

    /// <summary>Correct credentials, but the address was never confirmed.</summary>
    public string UnverifiedUser => "unverified@test.local";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Swap SQL Server for a SQLite connection that lives as long as this factory. The
            // connection has to be held open or the in-memory database is discarded with it.
            //
            // Since EF Core 9 the provider is registered through IDbContextOptionsConfiguration,
            // so dropping DbContextOptions alone leaves SQL Server registered and EF refuses to
            // start with two providers.
            services.RemoveAll<IDbContextOptionsConfiguration<EmployeeHubDbContext>>();
            services.RemoveAll<DbContextOptions<EmployeeHubDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<EmployeeHubDbContext>();

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddDbContext<EmployeeHubDbContext>(options => options.UseSqlite(_connection));
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EmployeeHubDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

        // The migrations are SQL Server-specific, so build the schema from the model instead.
        await context.Database.EnsureCreatedAsync();

        var department = new Department { Name = "Engineering", Description = "Builds things." };
        var position = new Position { Title = "Software Engineer", Description = "Writes software." };
        context.Departments.Add(department);
        context.Positions.Add(position);
        await context.SaveChangesAsync();

        var ada = NewEmployee("Ada", "Lovelace", department.Id, position.Id);
        var grace = NewEmployee("Grace", "Hopper", department.Id, position.Id);
        context.Employees.AddRange(ada, grace);
        await context.SaveChangesAsync();

        AdaEmployeeId = ada.Id;
        GraceEmployeeId = grace.Id;

        var adaLeave = NewLeaveRequest(ada.Id);
        var graceLeave = NewLeaveRequest(grace.Id);
        context.LeaveRequests.AddRange(adaLeave, graceLeave);

        context.Users.AddRange(
            NewUser(hasher, AdminUser, UserRole.Admin),
            NewUser(hasher, ManagerUser, UserRole.Manager),
            NewUser(hasher, EmployeeUser, UserRole.Employee, employeeId: ada.Id),
            NewUser(hasher, UnlinkedUser, UserRole.Employee),
            NewUser(hasher, UnverifiedUser, UserRole.Employee, emailVerified: false));

        await context.SaveChangesAsync();

        GraceLeaveRequestId = graceLeave.Id;
    }

    // Explicit implementation so xunit's Task-returning DisposeAsync does not collide with
    // WebApplicationFactory's ValueTask one. The connection is closed in Dispose below.
    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection?.Dispose();
        }

        base.Dispose(disposing);
    }

    private static Employee NewEmployee(string firstName, string lastName, int departmentId, int positionId) =>
        new()
        {
            FirstName = firstName,
            LastName = lastName,
            Email = $"{firstName.ToLowerInvariant()}@test.local",
            HireDate = new DateOnly(2026, 1, 15),
            DepartmentId = departmentId,
            PositionId = positionId
        };

    private static LeaveRequest NewLeaveRequest(int employeeId) =>
        new()
        {
            EmployeeId = employeeId,
            Type = LeaveType.Annual,
            Status = LeaveStatus.Pending,
            StartDate = new DateOnly(2026, 9, 1),
            EndDate = new DateOnly(2026, 9, 5),
            Reason = "Holiday",
            RequestedAt = DateTime.UtcNow
        };

    private static User NewUser(
        IPasswordHasher<User> hasher,
        string email,
        UserRole role,
        int? employeeId = null,
        bool emailVerified = true)
    {
        var user = new User
        {
            Email = email,
            Role = role,
            EmailVerified = emailVerified,
            EmployeeId = employeeId
        };
        user.PasswordHash = hasher.HashPassword(user, Password);

        return user;
    }
}
