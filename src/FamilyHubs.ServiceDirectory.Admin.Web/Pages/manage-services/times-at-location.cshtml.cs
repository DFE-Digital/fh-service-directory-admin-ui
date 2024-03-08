using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Common;
using FamilyHubs.ServiceDirectory.Admin.Web.Journeys;
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

    public string? Title { get; set; }

    public times_at_locationModel(
        IRequestDistributedCache connectionRequestCache)
        : base(ServiceJourneyPage.Times_At_Location, connectionRequestCache)
    {
    }

    protected override void OnGetWithError()
    {
        var location = ServiceModel!.CurrentLocation!;
        SetTitle(location);
    }

    protected override void OnGetWithModel()
    {
        //todo: redo mode will take user back to locations at service page
        //todo: how does redo work from details page?
        //todo: look for location id in url, if not there, work on current location

        string locationIdString = Request.Query["locationId"].ToString();
        string redo = Request.Query["redo"].ToString();

        if (redo != "")
        {
            BackUrl = $"{ServiceJourneyPageExtensions.GetPagePath(redo)}?flow={Flow}";
        }

        var location = ServiceModel!.CurrentLocation!;

        SetTitle(location);

        SelectedValues = location.Times ?? Enumerable.Empty<string>();
    }

    private void SetTitle(ServiceLocationModel location)
    {
        //todo: put the location display name somewhere common - sd shared?
        Title = $"On which days can people use this service at {location.DisplayName}?";
    }

    protected override IActionResult OnPostWithModel()
    {
        string locationIdString = Request.Query["locationId"].ToString();

        //todo: look for location id in url, if not there, work on current location
        ServiceModel!.Updated = ServiceModel!.Updated || HaveTimesAtLocationBeenUpdated();

        ServiceModel.CurrentLocation!.Times = SelectedValues;

        //todo: do here or in nextpage?

        string redo = Request.Query["redo"].ToString();
        if (redo != "")
        {
            return RedirectToServicePage(ServiceJourneyPageExtensions.FromSlug(redo), Flow);
        }

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