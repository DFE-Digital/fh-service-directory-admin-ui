using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages;

public class LogoutModel : PageModel
{
    private readonly ITokenService _tokenService;
    private readonly IAuthService _authService;
    private readonly IRedisCacheService _redisCacheService;

    public LogoutModel(ITokenService tokenService, IAuthService authService, IRedisCacheService redisCacheService)
    {
        _tokenService = tokenService;
        _authService = authService;
        _redisCacheService = redisCacheService;
    }

    public async Task<IActionResult> OnGet(string? returnUrl = null)
    {
        if (User != null && User.Identity != null && User.Identity.Name != null)
        {
            await _authService.RevokeToken(User.Identity.Name);
            _redisCacheService.ResetStringValue($"OrganisationId-{User.Identity.Name}");
        }
            

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        _tokenService.ClearTokens();

        if (returnUrl == null)
        {
            returnUrl = "/Index";
        }

        return LocalRedirect(returnUrl);
    }

    public async Task<IActionResult> OnPost(string returnUrl)
    {
        if (User != null && User.Identity != null && User.Identity.Name != null)
            await _authService.RevokeToken(User.Identity.Name);

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        _tokenService.ClearTokens();

        return LocalRedirect(returnUrl);
    }
}
