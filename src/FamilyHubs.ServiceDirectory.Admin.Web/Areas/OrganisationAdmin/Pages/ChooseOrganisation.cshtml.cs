using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.OrganisationAdmin.Pages;

public class ChooseOrganisationModel : PageModel
{
    [BindProperty]
    public long SelectedOrganisation { get; set; }
    public List<SelectListItem> Organisations { get; set; } = new List<SelectListItem>();
    public bool ValidationValid { get; set; } = true;

    private readonly IServiceDirectoryClient _serviceDirectoryClient;
    private readonly ICacheService _cacheService;

    public ChooseOrganisationModel(
        IServiceDirectoryClient serviceDirectoryClient, 
        ICacheService cacheService)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
        _cacheService = cacheService;
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
        
        var organisationViewModel = new OrganisationViewModel
        {
            Id = SelectedOrganisation
        };

        await _cacheService.StoreOrganisationWithService(organisationViewModel);

        return RedirectToPage("/OrganisationAdmin/Welcome", new
        {
            organisationId = SelectedOrganisation
        });

    }

    private async Task Init()
    {
        await _cacheService.StoreCurrentPageName("ChooseOrganisation");
        var allOrganisations = await _serviceDirectoryClient.GetListOrganisations();
        Organisations = allOrganisations.OrderBy(x => x.Name).Select(x =>
        {
            var item = new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            };
            return item;
        }).ToList();
    }
}