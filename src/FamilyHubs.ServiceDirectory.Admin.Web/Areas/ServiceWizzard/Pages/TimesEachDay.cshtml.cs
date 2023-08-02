using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.ServiceWizzard.Pages;

public class TimesEachDayModel : BasePageModel
{
    public bool IsSameTimeOnEachDay { get; set; }
    public List<string> DaySelection { get; set; } = default!;

    [BindProperty]
    public List<OpeningHours> OpeningHours { get; set; } = default!;

    [BindProperty]
    public bool ValidationValid { get; set; } = true;

    public TimesEachDayModel(IRequestDistributedCache requestCache)
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

        Init(viewModel);
    }

    public async Task<IActionResult> OnPostAddAnotherTime(string day)
    {
        OrganisationViewModel? viewModel = await GetOrganisationViewModel();
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

    public async Task<IActionResult> OnPostRemoveTime(int index)
    {
        OrganisationViewModel? viewModel = await GetOrganisationViewModel();
        if (viewModel != null)
        {
            Init(viewModel);

            if (index > -1) 
            {
                OpeningHours.RemoveAt(index);
            }

            OpeningHours = OpeningHours.OrderBy(x => x.SortOrder).ToList();
        }

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        OrganisationViewModel? viewModel = await GetOrganisationViewModel();
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

        if (!ModelState.IsValid || !ValidationValid)
        {
            ValidationValid = false;
            Init(viewModel);
            return Page();
        }

        viewModel.OpeningHours = OpeningHours;

        await SetCacheAsync(viewModel);

        return RedirectToPage("ServiceDeliveryType", new { area = "ServiceWizzard" });
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

            if (IsSameTimeOnEachDay)
            {
                if (!OpeningHours.Any())
                {
                    OpeningHours.Add(new Core.Models.OpeningHours
                    {
                        
                    });
                }

                for(int i = 0;  i < OpeningHours.Count; i++)
                {
                    OpeningHours[i].Day = i.ToString();
                }

            }
            else
            {
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


}
