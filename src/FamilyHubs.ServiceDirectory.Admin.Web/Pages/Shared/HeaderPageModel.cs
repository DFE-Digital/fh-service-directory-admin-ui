using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Razor.Header;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;

public class HeaderPageModel : PageModel, IFamilyHubsHeader
{
    public bool ShowActionLinks => IsAuthenticatedAndTermsAccepted;
    public bool ShowNavigationMenu => false;

    private bool IsAuthenticatedAndTermsAccepted =>
        User.Identity?.IsAuthenticated == true
        && HttpContext.TermsAndConditionsAccepted();
}