using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.ReferenceData.ICalendar;
using FamilyHubs.SharedKernel.Razor.FullPages.Checkboxes;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public static class DaysOfTheWeekCheckboxes
{
    public static Checkbox[] Checkboxes => new[]
    {
        new Checkbox("Monday", DayCode.MO.ToString()),
        new Checkbox("Tuesday", DayCode.TU.ToString()),
        new Checkbox("Wednesday", DayCode.WE.ToString()),
        new Checkbox("Thursday", DayCode.TH.ToString()),
        new Checkbox("Friday", DayCode.FR.ToString()),
        new Checkbox("Saturday", DayCode.SA.ToString()),
        new Checkbox("Sunday", DayCode.SU.ToString())
    };
}

public class times_at_locationModel : ServicePageModel, ICheckboxesPageModel
{

    public IEnumerable<ICheckbox> Checkboxes => DaysOfTheWeekCheckboxes.Checkboxes;

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
        SelectedValues = ServiceModel!.ServiceAtLocationTimes ?? Enumerable.Empty<string>();
    }

    protected override IActionResult OnPostWithModel()
    {
        ServiceModel!.Updated = ServiceModel!.Updated || HaveTimesAtLocationBeenUpdated();

        ServiceModel!.ServiceAtLocationTimes = SelectedValues;

        return NextPage();
    }

    private bool HaveTimesAtLocationBeenUpdated()
    {
        return ServiceModel!.ServiceAtLocationTimes != null &&
               !ServiceModel.ServiceAtLocationTimes
                   .OrderBy(x => x)
                   .SequenceEqual(SelectedValues.OrderBy(x => x));
    }
}