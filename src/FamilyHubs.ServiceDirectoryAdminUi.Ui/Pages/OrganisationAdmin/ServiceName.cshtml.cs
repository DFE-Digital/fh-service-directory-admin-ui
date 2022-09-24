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

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class ServiceNameModel : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "You must enter a service name")]
    public string ServiceName { get; set; } = default!;

    //[BindProperty]
    //public string? StrOrganisationViewModel { get; set; }

    [BindProperty]
    public bool ValidationValid { get; set; } = true;
    //public string SessionKeyService { get; private set; }

    private readonly IOpenReferralOrganisationAdminClientService _openReferralOrganisationAdminClientService;

    public ServiceNameModel(IOpenReferralOrganisationAdminClientService openReferralOrganisationAdminClientService)
    {
        _openReferralOrganisationAdminClientService = openReferralOrganisationAdminClientService;
    }

    public async Task OnGet(string organisationid, string serviceid, string strOrganisationViewModel)
    {
        /*** Using Session storage ***/
        //if service not stored in session, get from api, else get from session storage
        if (HttpContext.Session.Get<OrganisationViewModel>(SessionKeyService) == default)
        {
            OpenReferralOrganisationWithServicesDto openReferralOrganisation = await _openReferralOrganisationAdminClientService.GetOpenReferralOrganisationById(organisationid);
            var vm = ApiModelToViewModelHelper.CreateViewModel(openReferralOrganisation, serviceid);
            if (vm != null)
            {
                if (!string.IsNullOrEmpty(vm.ServiceName))
                    ServiceName = vm.ServiceName;
                HttpContext.Session.Set<OrganisationViewModel>(SessionKeyService, vm);
            }
        }
        else
        {
            var vm = HttpContext.Session.Get<OrganisationViewModel>(SessionKeyService);
            ServiceName = vm?.ServiceName ?? "";
        }

        //if (!string.IsNullOrEmpty(strOrganisationViewModel))
        //{
        //    StrOrganisationViewModel = strOrganisationViewModel;

        //    var organisationViewModel = JsonConvert.DeserializeObject<OrganisationViewModel>(StrOrganisationViewModel) ?? new OrganisationViewModel();
        //    if (organisationViewModel != null)
        //    {
        //        if (!string.IsNullOrEmpty(organisationViewModel.ServiceName))
        //            ServiceName = organisationViewModel.ServiceName;
        //    }
        //}
        //else
        //{
        //    OpenReferralOrganisationWithServicesDto openReferralOrganisation = await _openReferralOrganisationAdminClientService.GetOpenReferralOrganisationById(organisationid);
        //    var vm = ApiModelToViewModelHelper.CreateViewModel(openReferralOrganisation, serviceid);
        //    if (vm != null)
        //    {
        //        if (!string.IsNullOrEmpty(vm.ServiceName))
        //            ServiceName = vm.ServiceName;
        //        StrOrganisationViewModel = JsonConvert.SerializeObject(vm);
        //    }

        //}

    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid || ServiceName == null || ServiceName.Trim().Length == 0 || ServiceName.Length > 255)
        {
            ValidationValid = false;
            return Page();
        }

        var vm = HttpContext.Session.Get<OrganisationViewModel>(SessionKeyService);

        if (vm == null)
        {
            vm = new OrganisationViewModel();
        }
        vm.ServiceName = ServiceName;
        HttpContext.Session.Set(SessionKeyService, vm);

        //if (StrOrganisationViewModel != null)
        //{
        //    var organisationViewModel = JsonConvert.DeserializeObject<OrganisationViewModel>(StrOrganisationViewModel) ?? new OrganisationViewModel();

        //    organisationViewModel.ServiceName = ServiceName;

        //    StrOrganisationViewModel = JsonConvert.SerializeObject(organisationViewModel);
        //}

        //return RedirectToPage("/OrganisationAdmin/TypeOfService", new
        //{
        //    strOrganisationViewModel = StrOrganisationViewModel
        //});

        return Page();
    }
}
