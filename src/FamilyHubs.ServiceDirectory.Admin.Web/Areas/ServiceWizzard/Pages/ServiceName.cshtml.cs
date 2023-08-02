using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.ServiceWizzard.Pages;

public class ServiceNameModel : BasePageModel
{
    public string OrganisationId { get; set; } = default!;

    [BindProperty]
    [Required(ErrorMessage = "You must enter a service name")]
    public string ServiceName { get; set; } = default!;

    [BindProperty]
    public bool ValidationValid { get; set; } = true;

    public ServiceNameModel(IRequestDistributedCache requestCache)
        : base(requestCache)
    {
    }

    public async Task OnGet(string organisationid, string serviceid, string strOrganisationViewModel)
    {
        OrganisationViewModel? viewModel = await GetOrganisationViewModel();
        if (viewModel != null && viewModel.ServiceName != null)
        {
            ServiceName = viewModel.ServiceName;
        }
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid || ServiceName == null || ServiceName.Trim().Length == 0 || ServiceName.Length > 255)
        {
            ValidationValid = false;
            return Page();
        }

        OrganisationViewModel? viewModel = await GetOrganisationViewModel();
        if (viewModel == null)
        {
            viewModel = new OrganisationViewModel();
        }
        viewModel.ServiceName = ServiceName;
        await SetCacheAsync(viewModel);

        if (string.Compare(await GetLastPage(), "/CheckServiceDetails", StringComparison.OrdinalIgnoreCase) == 0)
        {
            return RedirectToPage("CheckServiceDetails", new { area = "ServiceWizzard" });
        }

        return RedirectToPage("TypeOfSupport", new { area = "ServiceWizzard" });

    }
}
