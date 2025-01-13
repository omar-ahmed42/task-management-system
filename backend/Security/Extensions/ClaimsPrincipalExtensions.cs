using System.Security.Claims;

namespace backend.Security.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal principal)
    {
        string? id = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return id == null ? null : Guid.Parse(id);
    }

    public static List<string> GetRoles(this ClaimsPrincipal principal)
    {
        ClaimsIdentity? claimsIdentity = (ClaimsIdentity?) principal.Identity;
        if (claimsIdentity == null) return [];

        return [.. claimsIdentity.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value)];
    }

    public static bool HasRole(this ClaimsPrincipal principal, string roleName) => principal.GetRoles().Contains(roleName);
    public static bool IsAdmin(this ClaimsPrincipal principal) => HasRole(principal, "admin");
}
