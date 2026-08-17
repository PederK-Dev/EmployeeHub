using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace EmployeeHub.Api.Tests;

public static class ApiClientExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>Signs in and returns a client that carries the resulting bearer token.</summary>
    public static async Task<HttpClient> SignedInAsAsync(this EmployeeHubApiFactory factory, string email)
    {
        var client = factory.CreateClient();
        var token = await GetTokenAsync(client, email, EmployeeHubApiFactory.Password);

        Assert.False(string.IsNullOrEmpty(token), $"Could not sign in as '{email}'.");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    /// <summary>Attempts a sign-in and returns the raw response, for tests asserting on failure.</summary>
    public static Task<HttpResponseMessage> LoginAsync(this HttpClient client, string email, string password) =>
        client.PostAsJsonAsync("/api/auth/login", new { email, password });

    public static async Task<string?> GetTokenAsync(HttpClient client, string email, string password)
    {
        var response = await client.LoginAsync(email, password);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);

        return body.TryGetProperty("token", out var token) ? token.GetString() : null;
    }

    public static async Task<JsonElement> ReadJsonAsync(this HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
}
