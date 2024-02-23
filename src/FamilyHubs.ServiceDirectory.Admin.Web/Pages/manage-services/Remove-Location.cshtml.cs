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
//    public long LocationId { get; set; }
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

    protected override Task OnGetWithErrorAsync(CancellationToken cancellationToken)
    {
        return OnGet(cancellationToken);
    }

    protected override Task OnGetWithModelAsync(CancellationToken cancellationToken)
    {
        return OnGet(cancellationToken);
    }

    private async Task OnGet(CancellationToken cancellationToken)
    {
        // alternative is for link to post back and store in model
        long locationId = long.Parse(Request.Query["locationId"]);

        //todo: will be
        //if (!ServiceModel!.LocationIds.Contains(locationId))
        if (ServiceModel!.CurrentLocation != locationId)
        {
            throw new InvalidOperationException("Location to remove not associated with service");
        }

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
            return RedirectToSelf(ErrorId.Remove_Location__MissingSelection);
        }

        bool removeLocation = bool.Parse(SelectedValue);

        if (removeLocation)
        {
            //todo: this will remove from list of locations
            ServiceModel!.CurrentLocation = null;
        }

        return RedirectToServicePage(ServiceJourneyPage.Locations_For_Service, Flow); // == JourneyFlow.AddRedo ? JourneyFlow.Add : Flow);
    }
}