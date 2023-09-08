using FamilyHubs.ServiceDirectory.Admin.Web.Extensions;
using FamilyHubs.SharedKernel.Razor.FamilyHubsUi.Options;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.Contact_Us;

public class IndexModel : PageModel
{
    public string PreviousPageLink { get; set; } = string.Empty;
    public IFamilyHubsUiOptions FamilyHubsUiOptions { get; set; }

    public IndexModel(IOptions<FamilyHubsUiOptions> familyHubsUiOptions)
    {
        FamilyHubsUiOptions = familyHubsUiOptions.Value;
    }

    public void OnGet()
    {
        PreviousPageLink = HttpContext.GetBackButtonPath();
    }
}