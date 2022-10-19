using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralOrganisations;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Extensions;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using static FamilyHubs.ServiceDirectoryAdminUi.Ui.Infrastructure.Configuration.SessionConfiguration;
using static FamilyHubs.ServiceDirectoryAdminUi.Ui.Infrastructure.Configuration.PageConfiguration;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class ServiceNameModel : PageModel
{
    public string LastPage { get; set; } = default!;
    public string UserFlow { get; set; } = default!;

    [BindProperty]
    [Required(ErrorMessage = "You must enter a service name")]
    public string ServiceName { get; set; } = default!;

    [BindProperty]
    public bool ValidationValid { get; set; } = true;

    private readonly IOpenReferralOrganisationAdminClientService _openReferralOrganisationAdminClientService;
    private readonly ISessionService _session;

    public ServiceNameModel(IOpenReferralOrganisationAdminClientService openReferralOrganisationAdminClientService, ISessionService sessionService)
    {
        _openReferralOrganisationAdminClientService = openReferralOrganisationAdminClientService;
        _session = sessionService;
    }

    public async Task OnGet(string organisationid, string serviceid, string strOrganisationViewModel)
    {
        LastPage = _session.RetrieveLastPageName(HttpContext);
        UserFlow = _session.RetrieveUserFlow(HttpContext);

        var sessionVm = _session.RetrieveOrganisationWithService(HttpContext);
        if (sessionVm != default)
        {
            ServiceName = sessionVm?.ServiceName ?? "";
        }
        if(sessionVm?.Uri == default)
        {
            OpenReferralOrganisationWithServicesDto openReferralOrganisation = await _openReferralOrganisationAdminClientService.GetOpenReferralOrganisationById(organisationid);
            var apiVm = ApiModelToViewModelHelper.CreateViewModel(openReferralOrganisation, serviceid);
            if (apiVm != null)
            {
                if (!string.IsNullOrEmpty(apiVm.ServiceName))
                    ServiceName = apiVm.ServiceName;
                _session.StoreOrganisationWithService(HttpContext, apiVm);
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

        var sessionVm = _session?.RetrieveOrganisationWithService(HttpContext);

        if (sessionVm == null)
        {
            sessionVm = new OrganisationViewModel();
        }
        sessionVm.ServiceName = ServiceName;
        _session?.StoreOrganisationWithService(HttpContext, sessionVm);

        if (_session?.RetrieveLastPageName(HttpContext) == CheckServiceDetailsPageName)
        {
            return RedirectToPage($"/OrganisationAdmin/{CheckServiceDetailsPageName}");
        }
        return RedirectToPage("/OrganisationAdmin/TypeOfService");

    }
}
