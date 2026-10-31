using System.Security.Claims;

namespace KormoKhoj.Services;

public static class ClaimsExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
        => int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
}
