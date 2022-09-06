using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralOrganisations;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class ServiceNameModel : PageModel
{
    [BindProperty]
    public string ServiceName { get; set; } = default!;

    [BindProperty]
    public string? StrOrganisationViewModel { get; set; }

    [BindProperty]
    public bool validationValid { get; set; } = true;

    private readonly IOpenReferralOrganisationAdminClientService _openReferralOrganisationAdminClientService;

    public ServiceNameModel(IOpenReferralOrganisationAdminClientService openReferralOrganisationAdminClientService)
    {
        _openReferralOrganisationAdminClientService = openReferralOrganisationAdminClientService;
    }

    public async Task OnGet(string organisationid, string serviceid, string strOrganisationViewModel)
    {
        if (!string.IsNullOrEmpty(strOrganisationViewModel))
        {
            StrOrganisationViewModel = strOrganisationViewModel;

            var organisationViewModel = JsonConvert.DeserializeObject<OrganisationViewModel>(StrOrganisationViewModel) ?? new OrganisationViewModel();
            if (organisationViewModel != null)
            {
                if (!string.IsNullOrEmpty(organisationViewModel.ServiceName))
                    ServiceName = organisationViewModel.ServiceName;
            }
        }
        else
        {
            OpenReferralOrganisationWithServicesDto openReferralOrganisation = await _openReferralOrganisationAdminClientService.GetOpenReferralOrganisationById(organisationid);
            var vm = ApiModelToViewModelHelper.CreateViewModel(openReferralOrganisation, serviceid);
            if (vm != null)
            {
                if (!string.IsNullOrEmpty(vm.ServiceName))
                    ServiceName = vm.ServiceName;
                StrOrganisationViewModel = JsonConvert.SerializeObject(vm);
            }

        }

    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid || ServiceName == null || ServiceName.Trim().Length == 0 || ServiceName.Length > 255)
        {
            validationValid = false;
            return Page();
        }

        if (StrOrganisationViewModel != null)
        {
            var organisationViewModel = JsonConvert.DeserializeObject<OrganisationViewModel>(StrOrganisationViewModel) ?? new OrganisationViewModel();

            organisationViewModel.ServiceName = ServiceName;
            //organisationViewModel.ServiceDescription = ServiceDescription;

            StrOrganisationViewModel = JsonConvert.SerializeObject(organisationViewModel);
        }

        return RedirectToPage("/OrganisationAdmin/TypeOfService", new
        {
            strOrganisationViewModel = StrOrganisationViewModel
        });
    }
}
