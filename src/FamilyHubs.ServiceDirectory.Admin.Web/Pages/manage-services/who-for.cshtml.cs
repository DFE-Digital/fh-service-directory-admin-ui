using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class WhoForViewModel
{
    //todo: rename
    public bool? Children { get; set; }
    public int FromAge { get; set; }
    public int ToAge { get; set; }
}

public class who_forModel : ServicePageModel<WhoForViewModel>
{
    //todo: if this works, could auto have a property in the base
    [BindProperty]
    public WhoForViewModel? ViewModel { get; set; }

    public IEnumerable<SelectListItem> MinimumAges => MinimumAgeOptions;
    public IEnumerable<SelectListItem> MaximumAges => MinimumAgeOptions.Concat(ExtraMaximumAgeOptions);

    private const int NoValueSelected = -1;

    public static SelectListItem[] MinimumAgeOptions { get; set; } =
    {
        new() { Value=$"{NoValueSelected}", Text="Select age", Selected = true, Disabled = true },
        new() { Value="0", Text="0 to 12 months"},
        new() { Value="1", Text="1 year old"},
        new() { Value="2", Text="2 years old"},
        new() { Value="3", Text="3 years old"},
        new() { Value="4", Text="4 years old"},
        new() { Value="5", Text="5 years old"},
        new() { Value="6", Text="6 years old"},
        new() { Value="7", Text="7 years old"},
        new() { Value="8", Text="8 years old"},
        new() { Value="9", Text="9 years old"},
        new() { Value="10", Text="10 years old"},
        new() { Value="11", Text="11 years old"},
        new() { Value="12", Text="12 years old"},
        new() { Value="13", Text="13 years old"},
        new() { Value="14", Text="14 years old"},
        new() { Value="15", Text="15 years old"},
        new() { Value="16", Text="16 years old"},
        new() { Value="17", Text="17 years old"},
        new() { Value="18", Text="18 years old"},
        new() { Value="19", Text="19 years old"},
        new() { Value="20", Text="20 years old"},
        new() { Value="21", Text="21 years old"},
        new() { Value="22", Text="22 years old"},
        new() { Value="23", Text="23 years old"},
        new() { Value="24", Text="24 years old"},
        new() { Value="25", Text="25 years old"},
    };

    private static SelectListItem[] ExtraMaximumAgeOptions { get; set; } =
    {
        //todo: do we need the 25 as well as the 25+?
        new() { Value = "127", Text = "25+ years old" }
    };

    public who_forModel(IRequestDistributedCache connectionRequestCache)
        : base(ServiceJourneyPage.Who_For, connectionRequestCache)
    {
    }

    protected override void OnGetWithError()
    {
        ViewModel = ServiceModel?.UserInput ?? throw new InvalidOperationException("ServiceModel.UserInput is null");
    }

    protected override void OnGetWithModel()
    {
        ViewModel = new WhoForViewModel
        {
            Children = ServiceModel!.ForChildren,
            FromAge = ServiceModel.MinimumAge ?? NoValueSelected,
            ToAge = ServiceModel.MaximumAge ?? NoValueSelected
        };
    }

    protected override IActionResult OnPostWithModel()
    {
        if (ViewModel == null)
        {
            throw new InvalidOperationException($"{nameof(ViewModel)} cannot be null");
        }

        if (ViewModel.Children == null)
        {
            return RedirectToSelf(ViewModel, ErrorId.Who_For__SelectChildrensService);
        }

        //todo: decompose

        if (ViewModel.Children == true)
        {
            if (ViewModel.FromAge == NoValueSelected || ViewModel.ToAge == NoValueSelected)
            {
                var errors = new List<ErrorId>();
                if (ViewModel.FromAge == NoValueSelected)
                {
                    errors.Add(ErrorId.Who_For__SelectFromAge);
                }

                if (ViewModel.ToAge == NoValueSelected)
                {
                    errors.Add(ErrorId.Who_For__SelectToAge);
                }

                return RedirectToSelf(ViewModel, errors.ToArray());
            }

            if (ViewModel.FromAge > ViewModel.ToAge)
            {
                return RedirectToSelf(ViewModel, ErrorId.Who_For__FromAgeAfterToAge);
            }

            if (ViewModel.FromAge == ViewModel.ToAge)
            {
                return RedirectToSelf(ViewModel, ErrorId.Who_For__SameAges);
            }
        }

        ServiceModel!.Updated = ServiceModel.Updated || HasWhoForBeenUpdated();

        ServiceModel!.ForChildren = ViewModel.Children;
        if (ViewModel.Children == true)
        {
            ServiceModel.MinimumAge = ViewModel.FromAge;
            ServiceModel.MaximumAge = ViewModel.ToAge;
        }
        else
        {
            ServiceModel.MinimumAge = ServiceModel.MaximumAge = null;
        }

        return NextPage();
    }

    private bool HasWhoForBeenUpdated()
    {
        // strictly speaking, we don't need to min & max age is not for children, but they should be null when ForChildren is null
        return ServiceModel!.ForChildren != ViewModel!.Children
               || ServiceModel.MinimumAge != ViewModel.FromAge
               || ServiceModel.MaximumAge != ViewModel.ToAge;
    }
}