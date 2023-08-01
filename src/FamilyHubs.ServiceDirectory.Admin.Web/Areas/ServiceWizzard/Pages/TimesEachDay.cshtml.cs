using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.ServiceWizzard.Pages;

public class TimesEachDayModel : PageModel
{
    private readonly IRequestDistributedCache _requestCache;

    public bool IsSameTimeOnEachDay { get; set; }
    public List<string> DaySelection { get; set; } = default!;

    [BindProperty]
    public List<OpeningHours> OpeningHours { get; set; } = default!;

    [BindProperty]
    public bool ValidationValid { get; set; } = true;

    public TimesEachDayModel(IRequestDistributedCache requestCache)
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

        Init(viewModel);
    }

    public async Task<IActionResult> OnPostAddAnotherTime(string day)
    {
        var user = HttpContext.GetFamilyHubsUser();
        OrganisationViewModel? viewModel = await _requestCache.GetAsync(user.Email);
        if (viewModel != null)
        {
            Init(viewModel);

            OpeningHours.Add(new Core.Models.OpeningHours
            {
                Day = day,
            });

            OpeningHours = OpeningHours.OrderBy(x => x.SortOrder).ToList();
        }

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        var user = HttpContext.GetFamilyHubsUser();
        OrganisationViewModel? viewModel = await _requestCache.GetAsync(user.Email);
        if (viewModel == null)
        {
            viewModel = new OrganisationViewModel();
        }

        foreach (var times in OpeningHours)
        {
            if (string.IsNullOrEmpty(times.Starts) || string.IsNullOrEmpty(times.Finishes))
            {
                ValidationValid = false;
                break;
            }
        }

        if (!ModelState.IsValid || ValidationValid)
        {
            ValidationValid = false;
            Init(viewModel);
            return Page();
        }

        return Page();
    }

    private void Init(OrganisationViewModel viewModel)
    {
        if (viewModel.IsSameTimeOnEachDay != null && viewModel.IsSameTimeOnEachDay.Value)
        {
            IsSameTimeOnEachDay = true;
        }

        DaySelection = viewModel.DaySelection;
        if (viewModel.OpeningHours != null)
        {
            OpeningHours = viewModel.OpeningHours;
        }
        else
        {
            if (OpeningHours == null)
            {
                OpeningHours = new List<OpeningHours>();
            }
            if (OpeningHours.Count != DaySelection.Count)
            {
                foreach (var day in DaySelection)
                {
                    OpeningHours.Add(new Core.Models.OpeningHours
                    {
                        Day = day,
                    });
                }
            }
        }
    }


}
