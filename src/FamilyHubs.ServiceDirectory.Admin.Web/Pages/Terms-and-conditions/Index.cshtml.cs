using FamilyHubs.ServiceDirectory.Admin.Web.Extensions;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.Terms_and_conditions;

public class IndexModel : HeaderPageModel
{
    public string PreviousPageLink { get; set; } = string.Empty;

    public void OnGet()
    {
        PreviousPageLink = HttpContext.GetBackButtonPath();
    }
}