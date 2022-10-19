using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralOrganisations;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class ListOrganisationsModel : PageModel
{
    private readonly IOpenReferralOrganisationAdminClientService _openReferralOrganisationAdminClientService;
    private readonly ISessionService _session;

    public ListOrganisationsModel(IOpenReferralOrganisationAdminClientService openReferralOrganisationAdminClientService, ISessionService sessionService
        )
    {
        _openReferralOrganisationAdminClientService = openReferralOrganisationAdminClientService;
        _session = sessionService;
    }
    
    public List<OpenReferralOrganisationDto> Organisations { get; private set; } = default!;

    public async Task OnGetAsync()
    {
        Organisations = await _openReferralOrganisationAdminClientService.GetListOpenReferralOrganisations();
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
