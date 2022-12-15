using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

[Authorize(Policy = "ServiceMaintainer")]
public class ServiceDeletedModel : PageModel
{
    private readonly IRedisCacheService _redis;

    public ServiceDeletedModel(IRedisCacheService redisCacheService)
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