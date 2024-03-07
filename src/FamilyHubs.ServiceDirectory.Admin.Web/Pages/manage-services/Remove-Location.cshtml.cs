using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.SharedKernel.Razor.FullPages.Radios;
using FamilyHubs.SharedKernel.Razor.FullPages.Radios.Common;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

//public class RemoveLocationRedirectModel
//{
//    public long LocationId { get; init; }
//}

public class Remove_LocationModel : ServicePageModel, IRadiosPageModel
{
    private readonly IServiceDirectoryClient _serviceDirectoryClient;
    public IEnumerable<IRadio> Radios => CommonRadios.YesNo;

    [BindProperty]
    public string? SelectedValue { get; set; }

    public string? DescriptionPartial => null;
    public string? Legend { get; private set; }

    //todo: add hint
    public LocationDto? Location { get; private set; }

    public Remove_LocationModel(
        IRequestDistributedCache connectionRequestCache,
        IServiceDirectoryClient serviceDirectoryClient)
        : base(ServiceJourneyPage.Remove_Location, connectionRequestCache)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    protected override async Task OnGetWithErrorAsync(CancellationToken cancellationToken)
    {
        await HandleGet(GetLocationId(), cancellationToken);
    }

    protected override async Task OnGetWithModelAsync(CancellationToken cancellationToken)
    {
        await HandleGet(GetLocationId(), cancellationToken);
    }

    private long GetLocationId()
    {
        // alternative is for link to post back and store in model
        long locationId = long.Parse(Request.Query["locationId"]);

        if (ServiceModel!.CurrentLocation?.Id == locationId
            || (ServiceModel!.Locations.Single(l => l.Id == locationId)) != null)
        {
            return locationId;
        }

        throw new InvalidOperationException("Location to remove not associated with service");
    }

    private async Task HandleGet(long locationId, CancellationToken cancellationToken)
    {
        Location = await _serviceDirectoryClient.GetLocationById(locationId, cancellationToken);
        Legend = $"Do you want to remove {Location.Name ?? string.Join(", ", Location.Address1, Location.Address2)} from this service?";

        BackUrl = GetServicePageUrl(ServiceJourneyPage.Locations_For_Service, Flow); // == JourneyFlow.AddRedo ? JourneyFlow.Add : Flow);
    }

    protected override IActionResult OnPostWithModel()
    {
        if (SelectedValue == null)
        {
            //todo: check the flow when redo'ing

            //todo: we could put the locationid in the usermodel, but it's probably better for the link on the locations for service page to postback and we'll store it in the cache
            // use the targeted postback?
            // but we'd have to change storage card to work with a submit button that looks like a link
            // we could store it in the cache in the get if it's not in the query params
            // or link to a new page that stores it in the cache and redirects to this page
            // ^^ go for this one as it's simple, and if we went with a submit button, it's still a postback followed by a redirect here, so the same as getting a new page and a redirect here, and we can have a real link.
            // the extra page can check the location id is associated with the service
            return RedirectToSelf(ErrorId.Remove_Location__MissingSelection);
        }

        bool removeLocation = bool.Parse(SelectedValue);

        if (removeLocation)
        {
            long locationId = GetLocationId();

            if (ServiceModel!.CurrentLocation?.Id == locationId)
            {
                ServiceModel.CurrentLocation = null;
            }
            else
            {
                //todo: get index by id and remove at index instead?
                ServiceModel.Locations.Remove(ServiceModel.Locations.Single(l => l.Id == locationId));
            }
        }

        return RedirectToServicePage(ServiceJourneyPage.Locations_For_Service, Flow); // == JourneyFlow.AddRedo ? JourneyFlow.Add : Flow);
    }
}