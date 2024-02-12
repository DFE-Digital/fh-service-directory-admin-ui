using System.Collections.ObjectModel;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Razor.FullPages.Checkboxes;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

//todo: move to shared
public enum DayCode
{
    MO, TU, WE, TH, FR, SA, SU
}

public class timesModel : ServicePageModel, ICheckboxesPageModel
{
    public static Checkbox[] StaticCheckboxes => new[]
    {
        new Checkbox("Monday", DayCode.MO.ToString()),
        new Checkbox("Tuesday", DayCode.TU.ToString()),
        new Checkbox("Wednesday", DayCode.WE.ToString()),
        new Checkbox("Thursday", DayCode.TH.ToString()),
        new Checkbox("Friday", DayCode.FR.ToString()),
        new Checkbox("Saturday", DayCode.SA.ToString()),
        new Checkbox("Sunday", DayCode.SU.ToString())
    };

    public IEnumerable<ICheckbox> Checkboxes => StaticCheckboxes;

    [BindProperty]
    public IEnumerable<string> SelectedValues { get; set; } = Enumerable.Empty<string>();

    public string? DescriptionPartial => "times-content";
    public string? Legend => "Select all the days when this service is available";
    public string? Hint => "Select all options that apply. If none apply or you do not know these yet, leave blank and click continue.";

    public bool ShowSelection { get; set; }

    public timesModel(IRequestDistributedCache connectionRequestCache)
        : base(ServiceJourneyPage.Times, connectionRequestCache)
    {
    }

    protected override void OnGetWithModel()
    {
        SelectedValues = ServiceModel!.Times ?? Enumerable.Empty<string>();
    }

    protected override IActionResult OnPostWithModel()
    {
        ServiceModel!.Updated = ServiceModel!.Updated || HaveTimesBeenUpdated();

        ServiceModel!.Times = SelectedValues;

        return NextPage();
    }

    private bool HaveTimesBeenUpdated()
    {
        return ServiceModel!.Times != null &&
               ServiceModel.Times
                   .OrderBy(x => x)
                   .SequenceEqual(SelectedValues.OrderBy(x => x));
    }
}