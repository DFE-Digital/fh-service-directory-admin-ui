using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Razor.FamilyHubsUi.Options;
using Microsoft.Extensions.Options;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.Terms_and_conditions;

public class IndexModel : HeaderPageModel
{
    public Uri ConnectPrivacyNoticeUrl { get; set; }
    public string SupportEmail { get; set;}

    public IndexModel(IOptions<FamilyHubsUiOptions> configuration)
    {
        var config = configuration.Value;
        ConnectPrivacyNoticeUrl = config.Url(UrlKeys.ConnectWeb, "/privacy");
        SupportEmail = config.SupportEmail;
    }
}