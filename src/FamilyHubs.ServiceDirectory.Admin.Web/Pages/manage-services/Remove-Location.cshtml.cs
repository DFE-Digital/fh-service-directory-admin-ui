using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Models.ServiceJourney;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.SharedKernel.Razor.FullPages.Radios;
using FamilyHubs.SharedKernel.Razor.FullPages.Radios.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using FamilyHubs.ServiceDirectory.Shared.Display;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class Remove_LocationModel : ServicePageModel, IRadiosPageModel
{
    private readonly IServiceDirectoryClient _serviceDirectoryClient;
    public IEnumerable<IRadio> Radios => CommonRadios.YesNo;

    [BindProperty]
    public string? SelectedValue { get; set; }

    public string? DescriptionPartial => "remove-location-content";
    public string? Legend => null;
    public string? Title { get; private set; }

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
        long locationId = long.Parse(Request.Query["locationId"]!);

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
        Title = $"Do you want to remove {Location.GetDisplayName()} from this service?";

        BackUrl = GetServicePageUrl(ServiceJourneyPage.Locations_For_Service);
    }

    protected override IActionResult OnPostWithModel()
    {
        long locationId = GetLocationId();

        if (SelectedValue == null)
        {
            var extraParams = new Dictionary<string, StringValues>
            {
                { "locationId", locationId.ToString() }
            };

            return RedirectToSelf(extraParams, ErrorId.Remove_Location__MissingSelection);
        }

        bool removeLocation = bool.Parse(SelectedValue);

        if (removeLocation)
        {
            if (ServiceModel!.CurrentLocation?.Id == locationId)
            {
                ServiceModel.CurrentLocation = null;
            }
            else
            {
                ServiceModel.Locations.Remove(ServiceModel.Locations.Single(l => l.Id == locationId));
            }

            if (ServiceModel.CurrentLocation == null
                && !ServiceModel.Locations.Any()
                && Flow == JourneyFlow.Add)
            {
                // user has removed all locations and is in the add flow
                return Redirect(GetServicePageUrl(ServiceJourneyPage.How_Use));
            }
        }

        return Redirect(GetServicePageUrl(ServiceJourneyPage.Locations_For_Service));
    }
}