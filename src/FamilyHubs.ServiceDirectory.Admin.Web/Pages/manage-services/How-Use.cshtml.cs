using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Models.ServiceJourney;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.SharedKernel.Razor.FullPages.Checkboxes;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class How_UseModel : ServicePageModel, ICheckboxesPageModel
{
    public static Checkbox[] StaticCheckboxes => new[]
    {
        new Checkbox("In person", AttendingType.InPerson.ToString()),
        new Checkbox("Online", AttendingType.Online.ToString()),
        new Checkbox("Telephone", AttendingType.Telephone.ToString())
    };

    public IEnumerable<ICheckbox> Checkboxes => StaticCheckboxes;

    [BindProperty]
    public IEnumerable<string> SelectedValues { get; set; } = Enumerable.Empty<string>();

    public string? DescriptionPartial => null;
    public string? Legend => "How can people use this service?";
    // have this string as the default?
    public string? Hint => "Select all options that apply.";

    public How_UseModel(IRequestDistributedCache connectionRequestCache)
        : base(ServiceJourneyPage.How_Use, connectionRequestCache)
    {
    }

    protected override void OnGetWithModel()
    {
        SelectedValues = ServiceModel!.HowUse?.Select(h => h.ToString()) ?? Enumerable.Empty<string>();
    }

    protected override IActionResult OnPostWithModel()
    {
        if (!SelectedValues.Any())
        {
            return RedirectToSelf(ErrorId.How_Use__MissingSelection);
        }

        var howUse = SelectedValues.Select(Enum.Parse<AttendingType>).ToArray();

        ServiceModel!.Updated = ServiceModel!.Updated || HasHowUseBeenUpdated(howUse);

        ServiceModel.HowUse = howUse;

        // the user might select in person, but not add any locations, so we still need the schedule
        //if (!howUse.Any(at => at is AttendingType.Online or AttendingType.Telephone))
        //{
        //    ServiceModel.Times = Enumerable.Empty<string>();
        //    ServiceModel.TimeDescription = null;
        //}

        return NextPage();
    }

    private bool HasHowUseBeenUpdated(AttendingType[] howUse)
    {
        return !ServiceModel!.HowUse
                   .OrderBy(x => x)
                   .SequenceEqual(howUse.OrderBy(x => x));
    }
}