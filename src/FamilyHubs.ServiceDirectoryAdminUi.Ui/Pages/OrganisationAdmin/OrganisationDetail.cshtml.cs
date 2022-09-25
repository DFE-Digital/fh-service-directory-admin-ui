using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class OrganisationDetailModel : PageModel
{
    private readonly IOpenReferralOrganisationAdminClientService _openReferralOrganisationAdminClientService;
    private readonly ISessionService _session;

    [BindProperty]
    public OrganisationViewModel OrganisationViewModel{ get; set; } = new OrganisationViewModel();

    public OrganisationDetailModel(IOpenReferralOrganisationAdminClientService openReferralOrganisationAdminClientService, ISessionService sessionService)
    {
        _openReferralOrganisationAdminClientService = openReferralOrganisationAdminClientService;
        _session = sessionService;
    }
    public async Task OnGetAsync(Guid? id)
    {
        if (id != null)
        {
            var organisation = await _openReferralOrganisationAdminClientService.GetOpenReferralOrganisationById(id.Value.ToString());
            OrganisationViewModel = new OrganisationViewModel
            {
                Id = id.Value,
                Name = organisation.Name,
                Description = organisation.Description,
                Uri = organisation.Uri,
                Url = organisation.Url,
                Logo = organisation.Logo

            };
            
        }   
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var strOrganisationViewModel = JsonConvert.SerializeObject(OrganisationViewModel);

        return RedirectToPage("/OrganisationAdmin/Welcome", new
        {
            strOrganisationViewModel
        });
    }
}
