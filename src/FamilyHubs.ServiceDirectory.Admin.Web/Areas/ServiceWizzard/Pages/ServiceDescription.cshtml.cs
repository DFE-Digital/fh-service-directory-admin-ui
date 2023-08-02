using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.ServiceWizzard.Pages;

public class ServiceDescriptionModel : PageModel
{
    private readonly IRequestDistributedCache _requestCache;

    [BindProperty]
    [MaxLength(500, ErrorMessage = "You can only add upto 500 characters")]
    public string? Description { get; set; }

    public ServiceDescriptionModel(IRequestDistributedCache requestCache)
    {
        _requestCache = requestCache;
    }

    public async Task OnGet()
    {
        var user = HttpContext.GetFamilyHubsUser();
        OrganisationViewModel? viewModel = await _requestCache.GetAsync(user.Email);
        if (viewModel == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(viewModel.ServiceDescription))
        {
            Description = viewModel.ServiceDescription;
        }    
    }

    public async Task<IActionResult> OnPost()
    {
        if (Description?.Length > 500)
        {
            ModelState.AddModelError(nameof(Description), "You can only add upto 500 characters");
            return Page();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = HttpContext.GetFamilyHubsUser();
        OrganisationViewModel? viewModel = await _requestCache.GetAsync(user.Email);
        if (viewModel == null)
        {
            viewModel = new OrganisationViewModel();
        }

        viewModel.ServiceDescription = Description;

        await _requestCache.SetAsync(user.Email, viewModel);

        return RedirectToPage("CheckServiceDetails", new { area = "ServiceWizzard" });
    }
}
