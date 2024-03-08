using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Common;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Razor.FullPages.Checkboxes;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class times_at_locationModel : ServicePageModel, ICheckboxesPageModel
{
    private readonly IServiceDirectoryClient _serviceDirectoryClient;
    public IEnumerable<ICheckbox> Checkboxes => CommonCheckboxes.DaysOfTheWeek;

    [BindProperty]
    public IEnumerable<string> SelectedValues { get; set; } = Enumerable.Empty<string>();

    //todo: either pass model to partial description, or have component partial take care of the header
    public string? DescriptionPartial => "times-at-location-content";
    public string? Legend => "Select any days when this service is available at this location";
    public string? Hint => "Select all options that apply. If none apply or you do not know these yet, leave blank and click continue.";

    public string? Title { get; set; }

    public times_at_locationModel(
        IRequestDistributedCache connectionRequestCache,
        IServiceDirectoryClient serviceDirectoryClient)
        : base(ServiceJourneyPage.Times_At_Location, connectionRequestCache)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    protected override async Task OnGetWithErrorAsync(CancellationToken cancellationToken)
    {
        var location = ServiceModel!.CurrentLocation!;
        await SetTitle(location.Id, cancellationToken);
    }

    protected override async Task OnGetWithModelAsync(CancellationToken cancellationToken)
    {
        //todo: redo mode will take user back to locations at service page
        //todo: how does redo work from details page?
        //todo: look for location id in url, if not there, work on current location

        var location = ServiceModel!.CurrentLocation!;

        await SetTitle(location.Id, cancellationToken);

        SelectedValues = location.Times ?? Enumerable.Empty<string>();
    }

    private async Task SetTitle(long locationId, CancellationToken cancellationToken)
    {
        var location = await _serviceDirectoryClient.GetLocationById(locationId, cancellationToken);

        //todo: put the location display name somewhere common - sd shared?
        Title = $"On which days can people use this service at {location.Name ?? string.Join(", ", location.Address1, location.Address2)}?";
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