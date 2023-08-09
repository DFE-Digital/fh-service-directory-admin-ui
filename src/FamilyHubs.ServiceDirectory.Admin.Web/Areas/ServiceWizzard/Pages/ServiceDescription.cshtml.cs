using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.ServiceWizzard.Pages;

public class ServiceDescriptionModel : BasePageModel
{
    [BindProperty]
    [MaxLength(500, ErrorMessage = "You can only add upto 500 characters")]
    public string? Description { get; set; }

    public ServiceDescriptionModel(IRequestDistributedCache requestCache)
        : base(requestCache)
    {
    }

    public async Task OnGet()
    {
        OrganisationViewModel? viewModel = await GetOrganisationViewModel();
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

        OrganisationViewModel? viewModel = await GetOrganisationViewModel();
        if (viewModel == null)
        {
            viewModel = new OrganisationViewModel();
        }

        viewModel.ServiceDescription = Description;

        await SetCacheAsync(viewModel);

        return RedirectToPage("CheckServiceDetails", new { area = "ServiceWizzard" });
    }
}
