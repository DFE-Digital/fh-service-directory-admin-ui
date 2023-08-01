using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.ServiceWizzard.Pages;

public class WhenServiceTakesPlaceModel : PageModel
{
    private readonly IRequestDistributedCache _requestCache;
    public List<string> DaySelection { get; set; } = default!;

    [BindProperty]
    public string RadioSelection { get; set; } = default!;

    [BindProperty]
    public bool ValidationValid { get; set; } = true;

    public WhenServiceTakesPlaceModel(IRequestDistributedCache requestCache)
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

        if (viewModel.DaySelection != null && viewModel.DaySelection.Any())
        {
            DaySelection = viewModel.DaySelection;
        }

        if (viewModel.IsSameTimeOnEachDay != null)
        {
            if (viewModel.IsSameTimeOnEachDay.Value)
            {
                RadioSelection = "Yes";
            }
            else
            {
                RadioSelection = "No";
            }
        } 
    }

    public async Task<IActionResult> OnPost()
    {
        var user = HttpContext.GetFamilyHubsUser();
        OrganisationViewModel? viewModel = await _requestCache.GetAsync(user.Email);
        if (viewModel == null)
        {
            viewModel = new OrganisationViewModel();
        }

        if (viewModel.DaySelection != null && viewModel.DaySelection.Any())
        {
            DaySelection = viewModel.DaySelection;
        }

        if (!ModelState.IsValid || string.IsNullOrEmpty(RadioSelection))
        {
            ValidationValid = false;
            return Page();
        }

       
        if (!string.IsNullOrEmpty(RadioSelection))
        {
            if (RadioSelection == "Yes")
            {
                viewModel.IsSameTimeOnEachDay = true;
            }
            else
            {
                viewModel.IsSameTimeOnEachDay = false;
            }
        }

        await _requestCache.SetAsync(user.Email, viewModel);

        return RedirectToPage("TimesEachDay", new { area = "ServiceWizzard" });

    }
}
