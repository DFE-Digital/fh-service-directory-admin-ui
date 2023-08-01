using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.ServiceWizzard.Pages;

public class ServiceNameModel : PageModel
{
    public string OrganisationId { get; set; } = default!;

    [BindProperty]
    [Required(ErrorMessage = "You must enter a service name")]
    public string ServiceName { get; set; } = default!;

    [BindProperty]
    public bool ValidationValid { get; set; } = true;

    private readonly IRequestDistributedCache _requestCache;

    public ServiceNameModel(IRequestDistributedCache requestCache)
    {
        _requestCache = requestCache;
    }

    public async Task OnGet(string organisationid, string serviceid, string strOrganisationViewModel)
    {
        var user = HttpContext.GetFamilyHubsUser();
        OrganisationViewModel? viewModel = await _requestCache.GetAsync(user.Email);
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

        var user = HttpContext.GetFamilyHubsUser();
        OrganisationViewModel? viewModel = await _requestCache.GetAsync(user.Email);
        if (viewModel == null)
        {
            viewModel = new OrganisationViewModel();
        }
        viewModel.ServiceName = ServiceName;
        await _requestCache.SetAsync(user.Email, viewModel);

        return RedirectToPage("TypeOfSupport", new { area = "ServiceWizzard" });

    }
}
