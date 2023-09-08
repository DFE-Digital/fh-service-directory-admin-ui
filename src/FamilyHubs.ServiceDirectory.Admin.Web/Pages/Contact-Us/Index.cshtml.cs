using FamilyHubs.ServiceDirectory.Admin.Web.Extensions;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Razor.FamilyHubsUi.Options;
using Microsoft.Extensions.Options;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.Contact_Us;

public class IndexModel : HeaderPageModel
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