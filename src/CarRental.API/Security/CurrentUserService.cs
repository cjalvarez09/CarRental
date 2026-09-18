using System.Security.Claims;
using CarRental.Application.Common.Interfaces;

namespace CarRental.API.Security;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public int? UserId =>
        int.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public string? Role => Principal?.FindFirstValue(ClaimTypes.Role);

    public int? CustomerId =>
        int.TryParse(Principal?.FindFirstValue("customerId"), out var id) ? id : null;
}
