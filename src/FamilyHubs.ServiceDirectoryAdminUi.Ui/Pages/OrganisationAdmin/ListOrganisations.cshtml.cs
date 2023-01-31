using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class ListOrganisationsModel : PageModel
{
    private readonly IOrganisationAdminClientService _organisationAdminClientService;
    private readonly ISessionService _session;
    private readonly IRedisCacheService _redis;

    public ListOrganisationsModel(IOrganisationAdminClientService organisationAdminClientService,
                                  ISessionService sessionService,
                                  IRedisCacheService redis)
    {
        _organisationAdminClientService = organisationAdminClientService;
        _session = sessionService;
        _redis = redis;
    }
    
    public List<OrganisationDto> Organisations { get; private set; } = default!;

    public async Task OnGetAsync()
    {
        Organisations = await _organisationAdminClientService.GetListOrganisations();
    }

    public IActionResult OnPostButton2()
    {
        Guid? idGuid = null;

        return RedirectToPage("/OrganisationAdmin/OrganisationDetail", new
        {
            id = idGuid,
        });
    }
}
