using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.ServiceWizzard.Pages;

public class WhenServiceTakesPlaceModel : BasePageModel
{
    public List<string> DaySelection { get; set; } = default!;

    [BindProperty]
    public string RadioSelection { get; set; } = default!;

    [BindProperty]
    public bool ValidationValid { get; set; } = true;

    public WhenServiceTakesPlaceModel(IRequestDistributedCache requestCache)
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
        OrganisationViewModel? viewModel = await GetOrganisationViewModel();
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

        await SetCacheAsync(viewModel);

        return RedirectToPage("TimesEachDay", new { area = "ServiceWizzard" });

    }
}
