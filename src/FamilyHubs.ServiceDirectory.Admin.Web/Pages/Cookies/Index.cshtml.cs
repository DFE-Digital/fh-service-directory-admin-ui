using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Razor.Cookies;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.Cookies;

public class IndexModel : HeaderPageModel
{
    public readonly ICookiePage CookiePage;

    public IndexModel(ICookiePage cookiePage)
    {
        CookiePage = cookiePage;
    }

    public void OnPost(bool analytics)
    {
        CookiePage.OnPost(analytics, Request, Response);
    }
}