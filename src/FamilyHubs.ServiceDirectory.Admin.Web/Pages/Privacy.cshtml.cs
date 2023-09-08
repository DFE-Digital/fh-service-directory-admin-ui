using FamilyHubs.ServiceDirectory.Admin.Web.Extensions;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages;

public class PrivacyModel : HeaderPageModel
{
    public string PreviousPageLink { get; set; } = string.Empty;

    public void OnGet()
    {
        PreviousPageLink = HttpContext.GetBackButtonPath();
    }
}