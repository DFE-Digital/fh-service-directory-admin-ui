using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Razor.ErrorNext;
using FamilyHubs.SharedKernel.Razor.FullPages.Checkboxes;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public enum DayCode
{
    MO, TU, WE, TH, FR, SA, SU
}

public class timesModel : ServicePageModel<TimesModels>, ICheckboxesPageModel
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

    //todo: do we want Errors in ICheckboxesPageModel?
    public IErrorState Errors { get; set; } = ErrorState.Empty;

    public string? DescriptionPartial => "Checkbox-Custom-Content";
    public string? Legend => "Select all the days when this service is available";
    public string? Hint => "Select all options that apply. If none apply or you do not know these yet, leave blank and click continue.";

    public bool ShowSelection { get; set; }

    public timesModel(IRequestDistributedCache connectionRequestCache)
        : base(ServiceJourneyPage.Times, connectionRequestCache)
    {
    }

    protected override void OnGetWithModel()
    {
        //todo: from cache
        SelectedValues = new[] { DayCode.MO.ToString(), DayCode.SU.ToString() };
    }

    protected override IActionResult OnPostWithModel()
    {
        //todo: into cache
        //ServiceModel!.Times = SelectedValues;

        return NextPage();
    }
}