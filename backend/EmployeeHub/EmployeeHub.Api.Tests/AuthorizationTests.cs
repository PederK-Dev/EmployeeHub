using System.Net;
using System.Net.Http.Json;

namespace EmployeeHub.Api.Tests;

/// <summary>
/// Proves that holding a valid token is not the same as being allowed in. Because anyone can
/// register, these are the tests that stop the directory leaking to the whole internet.
/// </summary>
public class AuthorizationTests : IClassFixture<EmployeeHubApiFactory>
{
    private readonly EmployeeHubApiFactory _factory;

    public AuthorizationTests(EmployeeHubApiFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("/api/employees")]
    [InlineData("/api/departments")]
    [InlineData("/api/positions")]
    [InlineData("/api/leave-requests")]
    [InlineData("/api/users")]
    [InlineData("/api/dashboard/stats")]
    public async Task Anonymous_callers_are_rejected(string route)
    {
        var response = await _factory.CreateClient().GetAsync(route);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/health")]
    public async Task Health_check_stays_anonymous(string route)
    {
        var response = await _factory.CreateClient().GetAsync(route);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Employee_cannot_read_the_directory()
    {
        var client = await _factory.SignedInAsAsync(_factory.EmployeeUser);

        var response = await client.GetAsync("/api/employees");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Manager_can_read_the_directory()
    {
        var client = await _factory.SignedInAsAsync(_factory.ManagerUser);

        var response = await client.GetAsync("/api/employees");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Employee_cannot_read_user_accounts()
    {
        var client = await _factory.SignedInAsAsync(_factory.EmployeeUser);

        var response = await client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Manager_cannot_read_user_accounts_either()
    {
        var client = await _factory.SignedInAsAsync(_factory.ManagerUser);

        var response = await client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_can_read_user_accounts()
    {
        var client = await _factory.SignedInAsAsync(_factory.AdminUser);

        var response = await client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/departments")]
    [InlineData("/api/positions")]
    public async Task Employee_can_read_but_not_write_org_metadata(string route)
    {
        var client = await _factory.SignedInAsAsync(_factory.EmployeeUser);

        var read = await client.GetAsync(route);
        var write = await client.PostAsJsonAsync(route, new { name = "Sneaky", title = "Sneaky" });

        Assert.Equal(HttpStatusCode.OK, read.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, write.StatusCode);
    }

    [Fact]
    public async Task Manager_can_write_org_metadata()
    {
        var client = await _factory.SignedInAsAsync(_factory.ManagerUser);

        var response = await client.PostAsJsonAsync("/api/departments", new { name = "Legal", description = "Contracts." });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
