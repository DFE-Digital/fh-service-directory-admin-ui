using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.ServiceWizzard.Pages;

public class DetailsSavedModelModel : BasePageModel
{
    public DetailsSavedModelModel(IRequestDistributedCache requestCache)
        : base(requestCache)
    {

    }
    public async Task OnGet()
    {
        await GetOrganisationViewModel();
    }

    public async Task<IActionResult> OnPost()
    {
        var organisation = await GetOrganisationViewModel();
        return RedirectToPage("Welcome", new { area = "ServiceWizzard", organisationId = organisation?.Id });
    }
}
