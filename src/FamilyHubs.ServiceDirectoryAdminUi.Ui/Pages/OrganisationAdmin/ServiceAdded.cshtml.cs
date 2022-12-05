using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralOrganisations;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class ServiceAddedModel : PageModel
{
    private readonly ISessionService _session;
    private readonly IRedisCacheService _redis;

    public ServiceAddedModel(ISessionService sessionService, IRedisCacheService redisCacheService)
    {
        _session = sessionService;
        _redis = redisCacheService;
    }
    public void OnGet()
    {   
        _redis.StoreCurrentPageName("ServiceAdded"); //TODO - replace page names with consts
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