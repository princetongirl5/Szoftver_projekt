using System.Security.Claims;

namespace EdzestervGenerator.Api.Services;

public static class RequestUser
{
    public static int? Id(ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : null;
    }
}
