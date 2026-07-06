using System.Security.Cryptography;

namespace EmployeeHub.Api.Services;

public static class TokenGenerator
{
    /// <summary>Creates a URL-safe random token for email verification / password reset links.</summary>
    public static string Create()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}
