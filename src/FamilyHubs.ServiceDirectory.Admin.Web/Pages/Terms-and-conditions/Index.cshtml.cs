using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Extensions;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Razor.FamilyHubsUi.Options;
using Microsoft.Extensions.Options;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.Terms_and_conditions;

public class IndexModel : HeaderPageModel
{
    private readonly FamilyHubsUiOptions _configuration;

    public string PreviousPageLink { get; set; } = string.Empty;
    public string ConnectPrivacyNoticeUrl { get; set; } = string.Empty;

    public string SupportEmail { get; set;} = string.Empty;

    public IndexModel(IOptions<FamilyHubsUiOptions> configuration)
    {
        _configuration = configuration.Value;
    }

    public void OnGet()
    {
        PreviousPageLink = HttpContext.GetBackButtonPath();
        ConnectPrivacyNoticeUrl = _configuration.Url(UrlKeys.ConnectWeb, "/privacy").ToString();
        SupportEmail = _configuration.SupportEmail;
    }
}