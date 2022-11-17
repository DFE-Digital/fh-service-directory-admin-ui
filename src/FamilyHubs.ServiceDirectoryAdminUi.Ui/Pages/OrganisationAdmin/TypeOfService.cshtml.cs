using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralTaxonomys;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using FamilyHubs.SharedKernel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using static FamilyHubs.ServiceDirectoryAdminUi.Ui.Infrastructure.Configuration.PageConfiguration;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;
public class TypeOfServiceModel : PageModel
{
    private readonly IOpenReferralOrganisationAdminClientService _openReferralOrganisationAdminClientService;
    private readonly ISessionService _session;
    private readonly IRedisCacheService _redis;

    public TypeOfServiceModel(IOpenReferralOrganisationAdminClientService openReferralOrganisationAdminClientService,
                                 ISessionService sessionService,
                                 IRedisCacheService redisCacheService)
    {
        _openReferralOrganisationAdminClientService = openReferralOrganisationAdminClientService;
        _session = sessionService;
        _redis = redisCacheService;
    }

    public string LastPage { get; set; } = default!;
    public string UserFlow { get; set; } = default!;
    public List<OpenReferralTaxonomyDto> OpenReferralTaxonomyRecords { get; private set; } = default!;

    [BindProperty]
    public List<string> TaxonomySelection { get; set; } = default!;

    public async Task OnGet()
    {
        //LastPage = _session.RetrieveLastPageName(HttpContext);
        //UserFlow = _session.RetrieveUserFlow(HttpContext);

        LastPage = _redis.RetrieveLastPageName();
        UserFlow = _redis.RetrieveUserFlow();

        await GetTaxonomiesAsync();

        //var organisationViewModel = _session.RetrieveOrganisationWithService(HttpContext) ?? new OrganisationViewModel();
        var organisationViewModel = _redis.RetrieveOrganisationWithService() ?? new OrganisationViewModel();

        if (organisationViewModel != null && organisationViewModel.TaxonomySelection != null && organisationViewModel.TaxonomySelection.Any())
        {
            TaxonomySelection = organisationViewModel.TaxonomySelection;
        }
    }

    public async Task<IActionResult> OnPost()
    {
        if (TaxonomySelection.Count() == 0)
        {
            ModelState.AddModelError(nameof(TaxonomySelection), "Please select one option");
        }

        if (!ModelState.IsValid)
        {
            await GetTaxonomiesAsync();
            return Page();
        }

        //var sessionVm = _session?.RetrieveOrganisationWithService(HttpContext) ?? new OrganisationViewModel();
        var sessionVm = _redis.RetrieveOrganisationWithService() ?? new OrganisationViewModel();

        sessionVm.TaxonomySelection = TaxonomySelection;
        _redis?.StoreOrganisationWithService(sessionVm);

        //if (_session?.RetrieveLastPageName(HttpContext) == CheckServiceDetailsPageName)
        if (_redis?.RetrieveLastPageName() == CheckServiceDetailsPageName)
        {
            return RedirectToPage($"/OrganisationAdmin/{CheckServiceDetailsPageName}");
        }
        return RedirectToPage("/OrganisationAdmin/ServiceDeliveryType");
    }

    private async Task GetTaxonomiesAsync()
    {
        PaginatedList<OpenReferralTaxonomyDto> taxonomies = await _openReferralOrganisationAdminClientService.GetTaxonomyList();

        if (taxonomies != null)
            OpenReferralTaxonomyRecords = new List<OpenReferralTaxonomyDto>(taxonomies.Items);
    }
}