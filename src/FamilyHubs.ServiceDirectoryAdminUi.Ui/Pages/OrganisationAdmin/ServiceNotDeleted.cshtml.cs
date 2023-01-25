using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class ServiceNotDeletedModel : PageModel
{
    private readonly IRedisCacheService _redis;

    public ServiceNotDeletedModel(IRedisCacheService redisCacheService)
    {
        _redis = redisCacheService;
    }
    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        var organisation = _redis.RetrieveOrganisationWithService();
        return RedirectToPage("/OrganisationAdmin/Welcome", new
        {
            organisationId = organisation?.Id,
        });
    }
}
