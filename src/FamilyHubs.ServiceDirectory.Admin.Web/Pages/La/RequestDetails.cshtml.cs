using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Razor.FamilyHubsUi.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using System.Net;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.SubjectAccessRequest.Pages.La;

[Authorize(Roles = $"{RoleTypes.LaDualRole},{RoleTypes.LaManager}")]
public class RequestDetailsModel : PageModel
{
    private readonly IReferralService _referralService;
    public ReferralDto Referral { get; set; } = default!;
    private readonly FamilyHubsUiOptions _familyHubsUiOptions;
    private readonly string _serviceUrl;

    public RequestDetailsModel(
        IReferralService referralService,
        IOptions<FamilyHubsUiOptions> familyHubsUiOptions)
    {
        _referralService = referralService;
        _familyHubsUiOptions = familyHubsUiOptions.Value;
        _serviceUrl = _familyHubsUiOptions.Url(UrlKeys.ConnectWeb,
            "ProfessionalReferral/LocalOfferDetail?serviceid=").ToString();
    }

    public async Task<IActionResult> OnGet(int id)
    {
        try
        {
            Referral = await _referralService.GetReferralById(id);
        }
        catch (ReferralClientServiceException ex)
        {
            // user has changed the id in the url to see a referral they shouldn't have access to
            if (ex.StatusCode == HttpStatusCode.Forbidden)
            {
                return Redirect(_familyHubsUiOptions.Url(UrlKeys.ManageWeb, "/Error/403").ToString());
            }
            throw;
        }
        return Page();
    }

    public string GetReferralServiceUrl(long serviceId)
    {
        return $"{_serviceUrl}{serviceId}";
    }
}
