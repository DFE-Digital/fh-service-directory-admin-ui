using System.ComponentModel.DataAnnotations;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class ServiceDescriptionModel : PageModel
{
    public string LastPage { get; set; } = default!;
    public string UserFlow { get; set; } = default!;

    private readonly ISessionService _session;
    private readonly IRedisCacheService _redis;

    [BindProperty]
    [MaxLength(500, ErrorMessage = "You can only add upto 500 characters")]
    public string? Description { get; set; }

    public ServiceDescriptionModel(ISessionService sessionService, IRedisCacheService redisCacheService)
    {
        _session = sessionService;
        _redis = redisCacheService;
    }
    public void OnGet(string strOrganisationViewModel)
    {
        LastPage = _redis.RetrieveLastPageName();
        UserFlow = _redis.RetrieveUserFlow();

        var organisationViewModel = _redis.RetrieveOrganisationWithService();

        if (organisationViewModel != null && !string.IsNullOrEmpty(organisationViewModel.ServiceDescription))
        {
            Description = organisationViewModel.ServiceDescription;
        }
    }

    public IActionResult OnPost()
    {
        if (Description?.Length > 500)
        {
            ModelState.AddModelError(nameof(Description), "You can only add upto 500 characters");
            return Page();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }
        
        var organisationViewModel = _redis.RetrieveOrganisationWithService();

        if (organisationViewModel != null)
            organisationViewModel.ServiceDescription = Description;

        _redis.StoreOrganisationWithService(organisationViewModel);

        return RedirectToPage("/OrganisationAdmin/CheckServiceDetails");

    }
}
