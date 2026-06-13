using System.Security.Claims;

namespace Timebash.API.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public Guid GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User
            ?? throw new UnauthorizedAccessException("HTTP context or user not available.");
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token.");

        return Guid.Parse(userIdClaim);
    }
}
