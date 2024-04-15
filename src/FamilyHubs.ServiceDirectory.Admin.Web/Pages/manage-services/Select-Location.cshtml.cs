using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Models.ServiceJourney;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Display;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

//when in AddRedoHowUse mode, and user adds a location, we need to come page in AddRedoHowUse mode
public class Select_LocationModel : ServicePageModel
{
    public const int NoSelectionLocationId = -1;
    public long? SelectedLocationId { get; private set; }
    public IEnumerable<LocationDto> Locations { get; private set; } = Enumerable.Empty<LocationDto>();
    public string? OrganisationType { get; private set; }

    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    // we ask for a maximum of 10000, as the front end is limited to 10000 anyway, see https://chromium.googlesource.com/chromium/blink.git/+/master/Source/core/html/HTMLSelectElement.cpp#77
    private const int MaxLocations = 10000;

    public Select_LocationModel(
        IServiceDirectoryClient serviceDirectoryClient,
        IRequestDistributedCache connectionRequestCache)
        : base(ServiceJourneyPage.Select_Location, connectionRequestCache)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    protected override async Task OnGetWithErrorAsync(CancellationToken cancellationToken)
    {
        await PopulateLocationsAndName(cancellationToken);
    }

    protected override async Task OnGetWithModelAsync(CancellationToken cancellationToken)
    {
        await PopulateLocationsAndName(cancellationToken);

        await UpdateCurrentLocationIfLocationJustAdded(cancellationToken);

        SelectedLocationId = ServiceModel!.CurrentLocation?.Id;
    }

    private async Task UpdateCurrentLocationIfLocationJustAdded(CancellationToken cancellationToken)
    {
        var locationIdString = Request.Query["locationId"];
        if (string.IsNullOrEmpty(locationIdString))
        {
            return;
        }

        long locationId = long.Parse(locationIdString!);
        if (locationId <= 0)
        {
            return;
        }

        var location = await _serviceDirectoryClient.GetLocationById(locationId, cancellationToken);
        ServiceModel!.CurrentLocation = new ServiceLocationModel(location);

        // we need to save to cache now, otherwise we lose the current location if the user hits back
        await Cache.SetAsync(FamilyHubsUser.Email, ServiceModel);
    }

    private async Task PopulateLocationsAndName(CancellationToken cancellationToken)
    {
        const string searchName = "";

        long organisationId = long.Parse(FamilyHubsUser.OrganisationId);

        if (FamilyHubsUser.Role == RoleTypes.DfeAdmin)
        {
            Locations = await GetAllLocations(searchName, cancellationToken);
        }
        else
        {
            Locations = await GetLocationsByOrganisation(searchName, organisationId, cancellationToken);

            OrganisationType = FamilyHubsUser.Role is RoleTypes.LaProfessional or RoleTypes.LaDualRole
                ? "local authority" : "organisation";
        }

        RemoveExistingLocationsFromSelection();

        foreach (var location in Locations)
        {
            // 'borrow' the description field to store the address
            location.Description = string.Join(", ", location.GetAddress());
        }

        Locations = Locations
            .OrderBy(l => l.Description);
    }

    private void RemoveExistingLocationsFromSelection()
    {
        // we don't remove the current location, as we need to preselect it
        var existingLocationIds = ServiceModel!.Locations
            .Select(l => l.Id)
            .ToHashSet();

        Locations = Locations
            .Where(l => !existingLocationIds.Contains(l.Id));
    }

    private async Task<string> GetOrganisationName(long organisationId, CancellationToken cancellationToken)
    {
        var organisation = await _serviceDirectoryClient.GetOrganisationById(organisationId, cancellationToken);
        return organisation.Name;
    }

    private async Task<List<LocationDto>> GetAllLocations(
        string searchName,
        CancellationToken cancellationToken)
    {
        //todo: should have a single PaginatedList?

        //todo: as an optimisation, could have a version without sorting etc.
        //todo: some of these are mandatory in the client, but not in the api - refactor params
        // passing "" as orderbyColumn should mean no ordering is done, which is ideal for us

        var locations = await _serviceDirectoryClient.GetLocations(
            true, "",
            searchName, false,
            1, MaxLocations, cancellationToken);

        return locations.Items;
    }

    private async Task<List<LocationDto>> GetLocationsByOrganisation(
        string searchName,
        long organisationId,
        CancellationToken cancellationToken)
    {
        var locations = await _serviceDirectoryClient.GetLocationsByOrganisationId(
            organisationId, true, "",
            searchName, false, 1, MaxLocations, cancellationToken);

        return locations.Items;
    }

    protected override async Task<IActionResult> OnPostWithModelAsync(CancellationToken cancellationToken)
    {
        //todo: BUG - after adding a location, and come back to this page, location is preselected. if user clears input box, then preselected location is used, rater than getting an error message
        string locationIdString = Request.Form["location"]!;

        if (!long.TryParse(locationIdString, out var locationId) || locationId == NoSelectionLocationId)
        {
            return RedirectToSelf(ErrorId.Select_Location__NoLocationSelected);
        }

        if (ServiceModel!.CurrentLocation == null
            || (ServiceModel!.CurrentLocation?.Id != null && locationId != ServiceModel!.CurrentLocation?.Id))
        {
            // either there isn't a current location, or the user has changed the current location (in which case we lose the extra location details)
            //todo: when editing a service, we don't want to set the current, just the location set
            ServiceModel!.CurrentLocation = await CreateServiceLocationModel(locationId, cancellationToken);
        }

        return NextPage();
    }

    private async Task<ServiceLocationModel> CreateServiceLocationModel(long locationId, CancellationToken cancellationToken)
    {
        var location = await _serviceDirectoryClient.GetLocationById(locationId, cancellationToken);
        if (location == null)
        {
            // it's possible that the location has been deleted since the user selected it
            throw new InvalidOperationException("Location not found");
        }

        return new ServiceLocationModel(location);
    }
}