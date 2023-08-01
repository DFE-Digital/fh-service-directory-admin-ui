using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.ServiceWizzard.Pages;

public class ServiceTimesModel : PageModel
{
    private readonly IRequestDistributedCache _requestCache;

    [BindProperty]
    public string HastimesChoice { get; set; } = default!;

    [BindProperty]
    public bool ValidationValid { get; set; } = true;

    public ServiceTimesModel(IRequestDistributedCache requestCache)
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

        var user = HttpContext.GetFamilyHubsUser();
        OrganisationViewModel? viewModel = await _requestCache.GetAsync(user.Email);
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
        

        await _requestCache.SetAsync(user.Email, viewModel);

        if (HastimesChoice == "Yes")
        {
            return RedirectToPage("DaysTakePlace", new { area = "ServiceWizzard" });
        }

        return RedirectToPage("ServiceDeliveryType", new { area = "ServiceWizzard" });
        
    }
}
