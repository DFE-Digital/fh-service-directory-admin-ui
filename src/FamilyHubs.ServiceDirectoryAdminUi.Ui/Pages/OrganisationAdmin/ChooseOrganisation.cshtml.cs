using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class ChooseOrganisationModel : PageModel
{
    [BindProperty]
    public string SelectedOrganisation { get; set; } = default!;
    public List<SelectListItem> Organisations { get; set; } = new List<SelectListItem>();
    public bool ValidationValid { get; set; } = true;

    private readonly IOpenReferralOrganisationAdminClientService _openReferralOrganisationAdminClientService;
    private readonly IRedisCacheService _redis;

    public ChooseOrganisationModel(IOpenReferralOrganisationAdminClientService openReferralOrganisationAdminClientService, IRedisCacheService redis)
    {
        _openReferralOrganisationAdminClientService = openReferralOrganisationAdminClientService;
        _redis = redis;
    }

    public async Task OnGet()
    {
        await Init();
    }

    public async Task<IActionResult> OnPost()
    {
        ValidationValid = ModelState.IsValid;
        if (string.IsNullOrEmpty(SelectedOrganisation))
        {
            ValidationValid = false;
        }
        if (!ValidationValid)
        {
            await Init();
            return Page();
        }

        if (Guid.TryParse(SelectedOrganisation, out var id))
        {
            OrganisationViewModel organisationViewModel = new()
            {
                Id = id
            };

            _redis.StoreOrganisationWithService(organisationViewModel);

            if (User != null && User.Identity != null)
                _redis.StoreStringValue($"OrganisationId-{User.Identity.Name}", SelectedOrganisation);

            return RedirectToPage("/OrganisationAdmin/Welcome", new
            {
                organisationId = SelectedOrganisation,
            });
        }

        await Init();
        return Page();
    }

    private async Task Init()
    {
        var allOrganisations = await _openReferralOrganisationAdminClientService.GetListOpenReferralOrganisations();
        Organisations = allOrganisations.OrderBy(x => x.Name).Select(x => new SelectListItem { Text = x.Name, Value = x.Id }).ToList();
    }
}