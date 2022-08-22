using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using LAHub.Domain.RecordEntities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class ListOrganisationsModel : PageModel
{
    private readonly IOpenReferralOrganisationAdminClientService _openReferralOrganisationAdminClientService;

    public ListOrganisationsModel(IOpenReferralOrganisationAdminClientService openReferralOrganisationAdminClientService
        )
    {
        _openReferralOrganisationAdminClientService = openReferralOrganisationAdminClientService;
    }

    public List<OpenReferralOrganisationRecord> Organisations { get; private set; } = default!;

    public async Task OnGetAsync()
    {
        Organisations = await _openReferralOrganisationAdminClientService.GetListOpenReferralOrganisations();
    }

    //public async Task<IActionResult> OnPostButton1()
    //{
    //    Organisations = await _openReferralOrganisationAdminClientService.GetListOpenReferralOrganisations();

    //    return Page();
    //}
    public IActionResult OnPostButton2()
    {
        Guid? idGuid = null;

        return RedirectToPage("/OrganisationAdmin/OrganisationDetail", new
        {
            id = idGuid,
        });
    }
}
