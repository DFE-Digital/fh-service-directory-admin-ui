using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models.Configuration;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;

public class CookieBannerViewModel : ICookieBannerViewModel
{
    private const string CookieConsentPath = "cookieConsent";
    private const string CookieDetailsPath = CookieConsentPath + "/details";

    private readonly ICookieBannerConfiguration _configuration;
    private readonly IUserContext _userContext;
    private readonly IUrlHelper _urlHelper;

    public CookieBannerViewModel(
        ICookieBannerConfiguration configuration,
        IUserContext userContext,
        IUrlHelper? urlHelper = null)
    {
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
        ArgumentNullException.ThrowIfNull(userContext, nameof(userContext));

        _urlHelper = urlHelper ?? new UrlHelper();

        _configuration = configuration;
        _userContext = userContext;
    }

    public string CookieConsentUrl => _urlHelper.GetPath(_userContext, _configuration.ManageFamilyHubBaseUrl, CookieConsentPath);

    public string CookieDetailsUrl => _urlHelper.GetPath(_userContext, _configuration.ManageFamilyHubBaseUrl, CookieDetailsPath);
}
