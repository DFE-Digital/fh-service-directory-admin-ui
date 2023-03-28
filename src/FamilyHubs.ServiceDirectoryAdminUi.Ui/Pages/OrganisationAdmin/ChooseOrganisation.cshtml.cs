using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class ChooseOrganisationModel : PageModel
{
    [BindProperty]
    public long SelectedOrganisation { get; set; } = default!;
    public List<SelectListItem> Organisations { get; set; } = new List<SelectListItem>();
    public bool ValidationValid { get; set; } = true;

    private readonly IOrganisationAdminClientService _organisationAdminClientService;
    private readonly IRedisCacheService _redis;

    public ChooseOrganisationModel(IOrganisationAdminClientService organisationAdminClientService, IRedisCacheService redis)
    {
        _organisationAdminClientService = organisationAdminClientService;
        _redis = redis;
    }

    public async Task OnGet()
    {
        await Init();
    }

    public async Task<IActionResult> OnPost()
    {
        ValidationValid = ModelState.IsValid;
        if (SelectedOrganisation < 1)
        {
            ValidationValid = false;
        }
        if (!ValidationValid)
        {
            await Init();
            return Page();
        }


        OrganisationViewModel organisationViewModel = new()
        {
            Id = SelectedOrganisation
        };

        _redis.StoreOrganisationWithService(organisationViewModel);

        if (User != null && User.Identity != null)
            _redis.StoreStringValue($"OrganisationId-{User.Identity.Name}", SelectedOrganisation.ToString());

        return RedirectToPage("/OrganisationAdmin/Welcome", new
        {
            organisationId = SelectedOrganisation,
        });

    }

    private async Task Init()
    {
        _redis.StoreCurrentPageName("ChooseOrganisation");
        var allOrganisations = await _organisationAdminClientService.GetListOrganisations();
        Organisations = allOrganisations.OrderBy(x => x.Name).Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() }).ToList();
    }
}