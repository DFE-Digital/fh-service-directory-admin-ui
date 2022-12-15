using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralOrganisations;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using static FamilyHubs.ServiceDirectoryAdminUi.Ui.Infrastructure.Configuration.PageConfiguration;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

[Authorize(Policy = "ServiceMaintainer")]
public class ServiceNameModel : PageModel
{
    public string LastPage { get; set; } = default!;
    public string UserFlow { get; set; } = default!;
    public string OrganisationId { get; set; } = default!;

    [BindProperty]
    [Required(ErrorMessage = "You must enter a service name")]
    public string ServiceName { get; set; } = default!;

    [BindProperty]
    public bool ValidationValid { get; set; } = true;

    private readonly IOpenReferralOrganisationAdminClientService _openReferralOrganisationAdminClientService;
    private readonly ISessionService _session;
    private readonly IRedisCacheService _redis;

    public ServiceNameModel(IOpenReferralOrganisationAdminClientService openReferralOrganisationAdminClientService, ISessionService sessionService, IRedisCacheService redisCacheService)
    {
        _openReferralOrganisationAdminClientService = openReferralOrganisationAdminClientService;
        _session = sessionService;
        _redis = redisCacheService;
    }

    public async Task OnGet(string organisationid, string serviceid, string strOrganisationViewModel)
    {
        OrganisationId = organisationid;
        LastPage = _redis.RetrieveLastPageName();
        UserFlow = _redis.RetrieveUserFlow();

        var sessionVm = _redis.RetrieveOrganisationWithService();
        if (sessionVm != null && organisationid == null) 
        {
            OrganisationId = sessionVm.Id.ToString();
        }
        

        if (sessionVm != default)
            ServiceName = sessionVm?.ServiceName ?? "";
        
        if(sessionVm?.Uri == default)
        {
            OpenReferralOrganisationWithServicesDto openReferralOrganisation = await _openReferralOrganisationAdminClientService.GetOpenReferralOrganisationById(organisationid ?? string.Empty);
            var apiVm = ApiModelToViewModelHelper.CreateViewModel(openReferralOrganisation, serviceid);
            if (apiVm != null)
            {
                if (!string.IsNullOrEmpty(apiVm.ServiceName))
                    ServiceName = apiVm.ServiceName;
                
                _redis.StoreOrganisationWithService(apiVm);
            }
        }

    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid || ServiceName == null || ServiceName.Trim().Length == 0 || ServiceName.Length > 255)
        {
            ValidationValid = false;
            return Page();
        }
        
        var sessionVm = _redis?.RetrieveOrganisationWithService();

        if (sessionVm == null)
        {
            sessionVm = new OrganisationViewModel();
        }
        
        sessionVm.ServiceName = ServiceName;
        _redis?.StoreOrganisationWithService(sessionVm);

        if (_redis?.RetrieveLastPageName() == CheckServiceDetailsPageName)
            return RedirectToPage($"/OrganisationAdmin/{CheckServiceDetailsPageName}");
        
        return RedirectToPage("/OrganisationAdmin/TypeOfService");

    }
}
