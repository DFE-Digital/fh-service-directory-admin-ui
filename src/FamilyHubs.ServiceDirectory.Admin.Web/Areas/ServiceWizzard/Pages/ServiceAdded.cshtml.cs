using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.ServiceWizzard.Pages;

public class ServiceAddedModel : BasePageModel
{
    public ServiceAddedModel(IRequestDistributedCache requestCache)
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
