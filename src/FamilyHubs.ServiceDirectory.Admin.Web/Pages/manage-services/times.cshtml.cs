using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models.ServiceJourney;
using FamilyHubs.ServiceDirectory.Admin.Web.Common;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.SharedKernel.Razor.FullPages.Checkboxes;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class timesModel : ServicePageModel, ICheckboxesPageModel
{
    public IEnumerable<ICheckbox> Checkboxes => CommonCheckboxes.DaysOfTheWeek;

    [BindProperty]
    public IEnumerable<string> SelectedValues { get; set; } = Enumerable.Empty<string>();

    public string? DescriptionPartial => "times-content";
    public string? Legend => "Select all the days when this service is available";
    public string? Hint => "Select all options that apply. If none apply or you do not know these yet, leave blank and click continue.";
    public string? Title { get; private set; }

    public timesModel(IRequestDistributedCache connectionRequestCache)
        : base(ServiceJourneyPage.Times, connectionRequestCache)
    {
    }

    protected override void OnGetWithModel()
    {
        if (ServiceModel!.HowUse.Contains(AttendingType.InPerson)
            && !ServiceModel.AllLocations.Any())
        {
            Title = "On which days can people use this service?";
        }
        else
        {
            bool online = ServiceModel.HowUse.Contains(AttendingType.Online);
            if (online && ServiceModel.HowUse.Contains(AttendingType.Telephone))
            {
                Title = "On which days can people use this service online or by telephone?";
            }
            else if (online)
            {
                Title = "On which days can people use this service online?";
            }
            else
            {
                Title = "On which days can people use this service by telephone?";
            }
        }

        SelectedValues = ServiceModel.Times ?? Enumerable.Empty<string>();
    }

    protected override IActionResult OnPostWithModel()
    {
        //todo: if no times are selected, need to set to Enumerable.Empty<string>()

        ServiceModel!.Updated = ServiceModel.Updated || HaveTimesBeenUpdated();

        ServiceModel.Times = SelectedValues;

        return NextPage();
    }

    private bool HaveTimesBeenUpdated()
    {
        var currentTimes = ServiceModel!.Times ?? Enumerable.Empty<string>();

        return !currentTimes
                   .OrderBy(x => x)
                   .SequenceEqual(SelectedValues.OrderBy(x => x));
    }
}