using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.ServiceWizzard.Pages;

public class ServiceTimesModel : BasePageModel
{
    [BindProperty]
    public string HastimesChoice { get; set; } = default!;

    [BindProperty]
    public bool ValidationValid { get; set; } = true;

    public ServiceTimesModel(IRequestDistributedCache requestCache)
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

        if (viewModel.HasSetDaysAndTimes != null) 
        {
            if (viewModel.HasSetDaysAndTimes.Value)
            {
                HastimesChoice = "Yes";
            }
            else
            {
                HastimesChoice = "No";
            }
        }
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid || string.IsNullOrEmpty(HastimesChoice))
        {
            ValidationValid = false;
            return Page();
        }

        OrganisationViewModel? viewModel = await GetOrganisationViewModel();
        if (viewModel == null)
        {
            viewModel = new OrganisationViewModel();
        }

        if (HastimesChoice == "Yes")
        {
            viewModel.HasSetDaysAndTimes = true;
        }
        else
        {
            viewModel.HasSetDaysAndTimes = false;
        }

        await SetCacheAsync(viewModel);

        if (HastimesChoice == "Yes")
        {
            return RedirectToPage("DaysTakePlace", new { area = "ServiceWizzard" });
        }

        return RedirectToPage("ServiceDeliveryType", new { area = "ServiceWizzard" });
        
    }
}
