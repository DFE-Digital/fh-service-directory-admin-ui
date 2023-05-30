using System.ComponentModel.DataAnnotations;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.OrganisationAdmin.Pages;

public class ServiceDescriptionModel : PageModel
{
    public string LastPage { get; set; } = default!;
    public string UserFlow { get; set; } = default!;

    private readonly ICacheService _cacheService;

    [BindProperty]
    [MaxLength(500, ErrorMessage = "You can only add up to 500 characters")]
    public string? Description { get; set; }

    public ServiceDescriptionModel(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }
    public async Task OnGet(string strOrganisationViewModel)
    {
        LastPage = await _cacheService.RetrieveLastPageName();
        UserFlow = await _cacheService.RetrieveUserFlow();

        var organisationViewModel = await _cacheService.RetrieveOrganisationWithService();

        if (organisationViewModel != null && !string.IsNullOrEmpty(organisationViewModel.ServiceDescription))
        {
            Description = organisationViewModel.ServiceDescription;
        }
    }

    public async Task<IActionResult> OnPost()
    {
        if (Description?.Length > 500)
        {
            ModelState.AddModelError(nameof(Description), "You can only add up to 500 characters");
            return Page();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }
        
        var organisationViewModel = await _cacheService.RetrieveOrganisationWithService();

        if (organisationViewModel != null)
            organisationViewModel.ServiceDescription = Description;

        await _cacheService.StoreOrganisationWithService(organisationViewModel);

        return RedirectToPage("/OrganisationAdmin/CheckServiceDetails");
    }
}
