using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.ServiceWizzard.Pages;

public class DaysTakePlaceModel : BasePageModel
{
    public Dictionary<string, string> DictDays { get; set; } = default!;

    [BindProperty]
    public List<string> DaySelection { get; set; } = default!;

    [BindProperty]
    public bool ValidationValid { get; set; } = true;
    public DaysTakePlaceModel(IRequestDistributedCache requestCache)
        : base(requestCache)
    {
       
    }
    public async Task OnGet()
    {
        PopulateDays();
        OrganisationViewModel? viewModel = await GetOrganisationViewModel();
        if (viewModel == null)
        {
            return;
        }

        if (viewModel.DaySelection != null && viewModel.DaySelection.Any())
        {
            DaySelection = viewModel.DaySelection;
        }
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid || DaySelection == null || !DaySelection.Any())
        {
            PopulateDays();
            ValidationValid = false;
            return Page();
        }

        OrganisationViewModel? viewModel = await GetOrganisationViewModel();
        if (viewModel == null)
        {
            viewModel = new OrganisationViewModel();
        }
        viewModel.DaySelection = DaySelection;

        await SetCacheAsync(viewModel);

        return RedirectToPage("WhenServiceTakesPlace", new { area = "ServiceWizzard" });
    }

    private void PopulateDays()
    {
        DictDays = new Dictionary<string, string>()
        {
            { "Monday", "Monday" },
            { "Tuesday", "Tuesday" },
            { "Wednesday", "Wednesday" },
            { "Thursday", "Thursday" },
            { "Friday", "Friday" },
            { "Saturday", "Saturday" },
            { "Sunday", "Sunday" },
        };
    }
}
