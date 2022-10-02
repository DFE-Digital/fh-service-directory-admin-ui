using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralTaxonomys;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using FamilyHubs.SharedKernel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using static FamilyHubs.ServiceDirectoryAdminUi.Ui.Infrastructure.Configuration.PageConfiguration;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class TypeOfServiceModel : PageModel
{
    private readonly IOpenReferralOrganisationAdminClientService _openReferralOrganisationAdminClientService;
    private readonly ISessionService _session;

    public List<OpenReferralTaxonomyDto> OpenReferralTaxonomyRecords { get; private set; } = default!;
    [BindProperty]
    public List<string> TaxonomySelection { get; set; } = default!;

    public TypeOfServiceModel(IOpenReferralOrganisationAdminClientService openReferralOrganisationAdminClientService, ISessionService sessionService)
    {
        _openReferralOrganisationAdminClientService = openReferralOrganisationAdminClientService;
        _session = sessionService;
    }

    public async Task OnGet()
    {
        PaginatedList<OpenReferralTaxonomyDto> taxonomies = await _openReferralOrganisationAdminClientService.GetTaxonomyList();

        if (taxonomies != null)
            OpenReferralTaxonomyRecords = new List<OpenReferralTaxonomyDto>(taxonomies.Items);

        var organisationViewModel = _session.RetrieveOrganisationWithService(HttpContext) ?? new OrganisationViewModel();
        if (organisationViewModel != null && organisationViewModel.TaxonomySelection != null && organisationViewModel.TaxonomySelection.Any())
        {
            TaxonomySelection = organisationViewModel.TaxonomySelection;
        }

        //var organisationViewModel = JsonConvert.DeserializeObject<OrganisationViewModel>(StrOrganisationViewModel) ?? new OrganisationViewModel();
        //if (organisationViewModel != null && organisationViewModel.TaxonomySelection != null && organisationViewModel.TaxonomySelection.Any())
        //{
        //    TaxonomySelection = organisationViewModel.TaxonomySelection;
        //}
    }

    public IActionResult OnPost()
    {
        /*** Using Session storage as a service ***/
        var sessionVm = _session?.RetrieveOrganisationWithService(HttpContext) ?? new OrganisationViewModel();
        sessionVm.TaxonomySelection = new List<string>(TaxonomySelection);
        _session?.StoreOrganisationWithService(HttpContext, sessionVm);

        if (_session.RetrieveLastPageName(HttpContext) == CheckServiceDetailsPageName)
        {
            return RedirectToPage($"/OrganisationAdmin/{CheckServiceDetailsPageName}");
        }
        return RedirectToPage("/OrganisationAdmin/ServiceDeliveryType");


        //if (StrOrganisationViewModel != null)
        //{
        //    var organisationViewModel = JsonConvert.DeserializeObject<OrganisationViewModel>(StrOrganisationViewModel) ?? new OrganisationViewModel();
        //    organisationViewModel.TaxonomySelection = new List<string>(TaxonomySelection);
        //    StrOrganisationViewModel = JsonConvert.SerializeObject(organisationViewModel);
        //}


        //return RedirectToPage("/OrganisationAdmin/ServiceDeliveryType", new
        //{
        //    strOrganisationViewModel = StrOrganisationViewModel
        //});
    }
}
