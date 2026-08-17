using System.Net;
using System.Net.Http.Json;

namespace EmployeeHub.Api.Tests;

/// <summary>
/// Registration is open to anyone, so it must not be a way to obtain a working session. These
/// tests pin the two properties that make that true: no token on the way out, and no sign-in
/// until the address is confirmed.
/// </summary>
public class RegistrationTests : IClassFixture<EmployeeHubApiFactory>
{
    private readonly EmployeeHubApiFactory _factory;

    public RegistrationTests(EmployeeHubApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Registering_does_not_hand_back_a_token()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new { email = "brand.new@test.local", password = EmployeeHubApiFactory.Password });

        response.EnsureSuccessStatusCode();
        var body = await response.ReadJsonAsync();

        Assert.False(body.TryGetProperty("token", out _), "Registration returned a token.");
    }

    [Fact]
    public async Task Registering_then_signing_in_immediately_is_refused()
    {
        var client = _factory.CreateClient();
        const string email = "unconfirmed@test.local";

        await client.PostAsJsonAsync("/api/auth/register", new { email, password = EmployeeHubApiFactory.Password });
        var login = await client.LoginAsync(email, EmployeeHubApiFactory.Password);

        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }

    [Fact]
    public async Task Unverified_account_cannot_sign_in_even_with_the_right_password()
    {
        var response = await _factory.CreateClient().LoginAsync(_factory.UnverifiedUser, EmployeeHubApiFactory.Password);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Verified_account_can_sign_in()
    {
        var response = await _factory.CreateClient().LoginAsync(_factory.EmployeeUser, EmployeeHubApiFactory.Password);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Wrong_password_is_rejected()
    {
        var response = await _factory.CreateClient().LoginAsync(_factory.AdminUser, "definitely-not-the-password");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Registering_an_existing_address_does_not_confirm_it_exists_by_succeeding()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new { email = _factory.AdminUser, password = EmployeeHubApiFactory.Password });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Forgot_password_reports_success_for_an_unknown_address()
    {
        var client = _factory.CreateClient();

        var known = await client.PostAsJsonAsync("/api/auth/forgot-password", new { email = _factory.AdminUser });
        var unknown = await client.PostAsJsonAsync("/api/auth/forgot-password", new { email = "nobody@test.local" });

        // Identical responses, so the endpoint cannot be used to enumerate registered addresses.
        Assert.Equal(HttpStatusCode.OK, known.StatusCode);
        Assert.Equal(HttpStatusCode.OK, unknown.StatusCode);
        Assert.Equal(await known.Content.ReadAsStringAsync(), await unknown.Content.ReadAsStringAsync());
    }
}
