using System.Security.Claims;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Interfaces;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
}
