using System.Security.Claims;
using EmployeeHub.Api.Models;

namespace EmployeeHub.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Reads the user id out of the token's subject claim. ASP.NET Core maps <c>sub</c> onto
    /// <see cref="ClaimTypes.NameIdentifier"/>, so check both.
    /// </summary>
    public static int? GetUserId(this ClaimsPrincipal principal)
    {
        var subject = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? principal.FindFirstValue("sub");

        return int.TryParse(subject, out var userId) ? userId : null;
    }

    /// <summary>
    /// True for roles that may see organisation-wide data (the employee directory, everyone's
    /// leave requests, per-department headcount) rather than only their own.
    /// </summary>
    public static bool CanViewAllStaff(this ClaimsPrincipal principal)
    {
        return principal.IsInRole(nameof(UserRole.Admin))
               || principal.IsInRole(nameof(UserRole.Manager));
    }
}
