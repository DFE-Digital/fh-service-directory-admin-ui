using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Common;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Razor.FullPages.Checkboxes;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class times_at_locationModel : ServicePageModel, ICheckboxesPageModel
{
    public IEnumerable<ICheckbox> Checkboxes => CommonCheckboxes.DaysOfTheWeek;

    [BindProperty]
    public IEnumerable<string> SelectedValues { get; set; } = Enumerable.Empty<string>();

    public string? DescriptionPartial => "times-at-location-content";
    public string? Legend => "Select any days when this service is available at this location";
    public string? Hint => "Select all options that apply. If none apply or you do not know these yet, leave blank and click continue.";

    public string? Title { get; set; } = "On which days can people use this service at [location]?";

    public times_at_locationModel(IRequestDistributedCache connectionRequestCache)
        : base(ServiceJourneyPage.Times_At_Location, connectionRequestCache)
    {
    }

    protected override void OnGetWithModel()
    {
        //todo: fill in title correctly
        //todo: redo mode will take user back to locations at service page
        //todo: how does redo work from details page?
        //todo: look for location id in url, if not there, work on current location
        SelectedValues = ServiceModel!.CurrentLocation!.Times ?? Enumerable.Empty<string>();
    }

    protected override IActionResult OnPostWithModel()
    {
        //todo: look for location id in url, if not there, work on current location
        ServiceModel!.Updated = ServiceModel!.Updated || HaveTimesAtLocationBeenUpdated();

        ServiceModel.CurrentLocation!.Times = SelectedValues;

        return NextPage();
    }

    private bool HaveTimesAtLocationBeenUpdated()
    {
        return ServiceModel!.CurrentLocation!.Times != null &&
               !ServiceModel.CurrentLocation.Times
                   .OrderBy(x => x)
                   .SequenceEqual(SelectedValues.OrderBy(x => x));
    }
}