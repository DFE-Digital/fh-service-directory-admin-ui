using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.ServiceWizzard.Pages;

public class DaysTakePlaceModel : PageModel
{
    private readonly IRequestDistributedCache _requestCache;

    public Dictionary<string, string> DictDays { get; set; } = default!;

    [BindProperty]
    public List<string> DaySelection { get; set; } = default!;

    [BindProperty]
    public bool ValidationValid { get; set; } = true;
    public DaysTakePlaceModel(IRequestDistributedCache requestCache)
    {
        _requestCache = requestCache;     
    }
    public async Task OnGet()
    {
        PopulateDays();
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
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid || DaySelection == null || !DaySelection.Any())
        {
            PopulateDays();
            ValidationValid = false;
            return Page();
        }

        var user = HttpContext.GetFamilyHubsUser();
        OrganisationViewModel? viewModel = await _requestCache.GetAsync(user.Email);
        if (viewModel == null)
        {
            viewModel = new OrganisationViewModel();
        }
        viewModel.DaySelection = DaySelection;

        await _requestCache.SetAsync(user.Email, viewModel);

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
