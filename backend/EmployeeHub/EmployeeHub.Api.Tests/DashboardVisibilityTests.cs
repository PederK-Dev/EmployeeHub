namespace EmployeeHub.Api.Tests;

/// <summary>
/// The dashboard is readable by everyone, so the directory-derived sections are trimmed out of
/// the response for non-managers rather than merely hidden by the UI.
/// </summary>
public class DashboardVisibilityTests : IClassFixture<EmployeeHubApiFactory>
{
    private readonly EmployeeHubApiFactory _factory;

    public DashboardVisibilityTests(EmployeeHubApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Manager_receives_the_named_employee_sections()
    {
        var client = await _factory.SignedInAsAsync(_factory.ManagerUser);

        var stats = await (await client.GetAsync("/api/dashboard/stats")).ReadJsonAsync();

        Assert.NotEmpty(stats.GetProperty("recentEmployees").EnumerateArray());
        Assert.NotEmpty(stats.GetProperty("employeesByDepartment").EnumerateArray());
    }

    [Fact]
    public async Task Employee_receives_counts_but_no_names()
    {
        var client = await _factory.SignedInAsAsync(_factory.EmployeeUser);

        var stats = await (await client.GetAsync("/api/dashboard/stats")).ReadJsonAsync();

        Assert.Empty(stats.GetProperty("recentEmployees").EnumerateArray());
        Assert.Empty(stats.GetProperty("employeesByDepartment").EnumerateArray());

        // The headline counts are not personal data and stay visible to everyone.
        Assert.True(stats.GetProperty("employeeCount").GetInt32() > 0);
    }
}
