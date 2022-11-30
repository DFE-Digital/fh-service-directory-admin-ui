using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Framework;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class ChooseOrganisationModel : PageModel
{
    private readonly IOpenReferralOrganisationAdminClientService _openReferralOrganisationAdminClientService;
    private readonly IRedisCacheService _redis;

    [Required]
    [BindProperty]
    public string SelectedOrganisation { get; set; } = default!;
    public List<SelectListItem> Organisations { get; set; } = new List<SelectListItem>();
    public bool ValidationValid { get; set; } = true;

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
        if (!ModelState.IsValid)
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
        }
        else
        {
            ValidationValid = false;
            ModelState.AddModelError("SelectedOrganisation", "Failed to Parse Id, please contact administrator");
            return Page();
        }

        if (User != null && User.Identity != null)
            _redis.StoreStringValue($"OrganisationId-{User.Identity.Name}", SelectedOrganisation);

        return RedirectToPage("/OrganisationAdmin/Welcome", new
        {
            organisationId = SelectedOrganisation,
        });
    }

    private async Task Init()
    {
        var orgainsations = await _openReferralOrganisationAdminClientService.GetListOpenReferralOrganisations();
        if (orgainsations != null)
        {
            Organisations = orgainsations.OrderBy(x => x.Name).Select(x => new SelectListItem { Text = x.Name, Value = x.Id }).ToList();
        }
    }
}
