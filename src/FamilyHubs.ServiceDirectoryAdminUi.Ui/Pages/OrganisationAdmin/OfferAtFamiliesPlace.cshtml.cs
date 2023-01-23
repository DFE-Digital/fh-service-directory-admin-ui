using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class OfferAtFamiliesPlaceModel : PageModel
{
    private readonly ISessionService _session;
    private readonly IRedisCacheService _redis;

    [BindProperty]
    public string Familychoice { get; set; } = default!;

    [BindProperty]
    public OrganisationViewModel OrganisationViewModel { get; set; } = new OrganisationViewModel();

    [BindProperty]
    public bool ValidationValid { get; set; } = true;

    public OfferAtFamiliesPlaceModel(ISessionService sessionService, IRedisCacheService redis)
    {
        _session = sessionService;
        _redis = redis;
    }
    public void OnGet(string strOrganisationViewModel)
    {
        
        OrganisationViewModel = _redis.RetrieveOrganisationWithService() ?? new OrganisationViewModel();

        if (!string.IsNullOrEmpty(OrganisationViewModel.Familychoice))
        {
            Familychoice = OrganisationViewModel.Familychoice;
        }

    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            ValidationValid = false;
            return Page();
        }

        OrganisationViewModel = _redis.RetrieveOrganisationWithService() ?? new OrganisationViewModel();
        OrganisationViewModel.Familychoice = Familychoice;
        _redis.StoreOrganisationWithService(OrganisationViewModel);
        return RedirectToPage("/OrganisationAdmin/WhoFor");

    }
}