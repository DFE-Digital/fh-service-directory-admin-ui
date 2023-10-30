using FamilyHubs.ServiceDirectory.Admin.Web.Extensions;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Razor.FamilyHubsUi.Options;
using Microsoft.Extensions.Options;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages;

public class AccessibilityModel : HeaderPageModel
{
    private readonly FamilyHubsUiOptions _configuration;

    public string PreviousPageLink { get; set; } = string.Empty;
    public string SupportEmail { get; set; } = string.Empty;
    public AccessibilityModel(IOptions<FamilyHubsUiOptions> configuration)
    {
        _configuration = configuration.Value;
    }
    public void OnGet()
    {
        PreviousPageLink = HttpContext.GetBackButtonPath();
        SupportEmail = _configuration.SupportEmail;
    }
}