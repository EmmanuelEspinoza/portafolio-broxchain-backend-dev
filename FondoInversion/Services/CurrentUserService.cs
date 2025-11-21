
using System.Security.Claims;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetUserId()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        var claim = principal?.FindFirst(ClaimTypes.NameIdentifier) 
                    ?? principal?.FindFirst("sub") 
                    ?? throw new UnauthorizedAccessException("Usuario no autenticado.");

        return claim.Value;
    }

    public string GetUserEmail()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        return principal?.FindFirst(ClaimTypes.Email)?.Value;
    }
}