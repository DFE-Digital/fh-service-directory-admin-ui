using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Razor.FamilyHubsUi.Options;
using Microsoft.Extensions.Options;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages;

public class AccessibilityModel : HeaderPageModel
{
    public string SupportEmail { get; set; }

    public AccessibilityModel(IOptions<FamilyHubsUiOptions> configuration)
    {
        SupportEmail = configuration.Value.SupportEmail;
    }
}