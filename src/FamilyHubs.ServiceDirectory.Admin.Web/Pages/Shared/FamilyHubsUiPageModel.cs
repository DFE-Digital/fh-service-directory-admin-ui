using FamilyHubs.SharedKernel.Razor.FamilyHubsUi.Options;
using Microsoft.Extensions.Options;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;

//todo: move to web components
public class FamilyHubsUiPageModel : HeaderPageModel
{
    public IFamilyHubsUiOptions FamilyHubsUiOptions { get; }

    public FamilyHubsUiPageModel(IOptions<FamilyHubsUiOptions> familyHubsUiOptions)
    {
        FamilyHubsUiOptions = familyHubsUiOptions.Value;
    }
}