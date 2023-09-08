using FamilyHubs.SharedKernel.Razor.Header;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;

public class HeaderPageModel : PageModel, IFamilyHubsHeader
{
    public bool ShowActionLinks => User.Identity?.IsAuthenticated == true;
    public bool ShowNavigationMenu => false;
}