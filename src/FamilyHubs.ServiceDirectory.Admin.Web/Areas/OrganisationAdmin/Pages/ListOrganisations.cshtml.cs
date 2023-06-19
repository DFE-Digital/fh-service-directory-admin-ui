using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.OrganisationAdmin.Pages;

public class ListOrganisationsModel : PageModel
{
    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public ListOrganisationsModel(IServiceDirectoryClient serviceDirectoryClient)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    public List<OrganisationDto> Organisations { get; private set; } = default!;

    public async Task OnGetAsync()
    {
        Organisations = await _serviceDirectoryClient.GetCachedLaOrganisations();
    }

    public IActionResult OnPostButton2()
    {
        Guid? idGuid = null;

        return RedirectToPage("/OrganisationAdmin/OrganisationDetail", new { id = idGuid });
    }
}
