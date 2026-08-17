using System.Net;
using System.Net.Http.Json;

namespace EmployeeHub.Api.Tests;

/// <summary>
/// Leave records are personal data that every signed-in user can reach, so the filtering happens
/// per-caller inside the service rather than through an attribute. These tests cover both
/// directions: seeing other people's leave, and acting in their name.
/// </summary>
public class LeaveRequestScopingTests : IClassFixture<EmployeeHubApiFactory>
{
    private readonly EmployeeHubApiFactory _factory;

    public LeaveRequestScopingTests(EmployeeHubApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Manager_sees_everyones_leave()
    {
        var client = await _factory.SignedInAsAsync(_factory.ManagerUser);

        var body = await (await client.GetAsync("/api/leave-requests")).ReadJsonAsync();
        var names = body.EnumerateArray().Select(r => r.GetProperty("employeeName").GetString()).ToList();

        Assert.Contains("Ada Lovelace", names);
        Assert.Contains("Grace Hopper", names);
    }

    [Fact]
    public async Task Employee_sees_only_their_own_leave()
    {
        var client = await _factory.SignedInAsAsync(_factory.EmployeeUser);

        var body = await (await client.GetAsync("/api/leave-requests")).ReadJsonAsync();
        var names = body.EnumerateArray().Select(r => r.GetProperty("employeeName").GetString()).ToList();

        Assert.Contains("Ada Lovelace", names);
        Assert.DoesNotContain("Grace Hopper", names);
    }

    [Fact]
    public async Task Account_with_no_employee_record_sees_nothing()
    {
        var client = await _factory.SignedInAsAsync(_factory.UnlinkedUser);

        var body = await (await client.GetAsync("/api/leave-requests")).ReadJsonAsync();

        Assert.Empty(body.EnumerateArray());
    }

    [Fact]
    public async Task Employee_fetching_someone_elses_request_by_id_gets_not_found()
    {
        var client = await _factory.SignedInAsAsync(_factory.EmployeeUser);

        var response = await client.GetAsync($"/api/leave-requests/{_factory.GraceLeaveRequestId}");

        // NotFound rather than Forbidden, so ids cannot be probed for existence.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Employee_cannot_file_leave_in_someone_elses_name()
    {
        var client = await _factory.SignedInAsAsync(_factory.EmployeeUser);

        var response = await client.PostAsJsonAsync("/api/leave-requests", NewRequest(_factory.GraceEmployeeId));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Employee_can_file_leave_for_themselves()
    {
        var client = await _factory.SignedInAsAsync(_factory.EmployeeUser);

        var response = await client.PostAsJsonAsync("/api/leave-requests", NewRequest(_factory.AdaEmployeeId));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Unlinked_account_cannot_file_leave_at_all()
    {
        var client = await _factory.SignedInAsAsync(_factory.UnlinkedUser);

        var response = await client.PostAsJsonAsync("/api/leave-requests", NewRequest(_factory.AdaEmployeeId));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Employee_cannot_approve_leave()
    {
        var client = await _factory.SignedInAsAsync(_factory.EmployeeUser);

        var response = await client.PutAsJsonAsync(
            $"/api/leave-requests/{_factory.GraceLeaveRequestId}/status",
            new { status = "Approved" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Employee_cannot_delete_leave()
    {
        var client = await _factory.SignedInAsAsync(_factory.EmployeeUser);

        var response = await client.DeleteAsync($"/api/leave-requests/{_factory.GraceLeaveRequestId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static object NewRequest(int employeeId) => new
    {
        employeeId,
        type = "Sick",
        startDate = "2026-10-01",
        endDate = "2026-10-02",
        reason = "Flu"
    };
}
