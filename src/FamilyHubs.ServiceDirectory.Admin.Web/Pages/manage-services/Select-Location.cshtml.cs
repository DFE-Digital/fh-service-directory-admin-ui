using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Models.ServiceJourney;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Display;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Razor.FullPages.SingleAutocomplete;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class Select_LocationModel : ServicePageModel, ISingleAutocompletePageModel
{
    public const int NoSelectionId = -1;

    public string? ContentTop => "Select-Location-Content-Top";
    public string? ContentBottom => "Select-Location-Content-Bottom";
    public IReadOnlyDictionary<string, HtmlString>? ContentSubstitutions { get; private set; }

    [BindProperty]
    public string? SelectedValue { get; set; }
    public string Label => "Search and select a location to add to this service";
    public string? DisabledOptionValue => NoSelectionId.ToString();
    public IEnumerable<ISingleAutocompleteOption> Options { get; private set; } = Enumerable.Empty<ISingleAutocompleteOption>();

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
        await PopulateLocationsAndSubstitutions(cancellationToken);
    }

    /// <summary>
    /// The scenarios we have to handle for this page are many and tricky,
    /// so we handle them all here, rather than in the base class.
    ///
    /// We now generate a sensible back link in all scenarios (though it's still not perfect).
    /// Scenarios - user could have come from:
    /// 1) add location page (add/edit)x(redo how)
    /// 2) locations for service page (add/edit)x(initial add[not edit]/redo location/redo how use)x(first time/subsequent 'add location' loop/after removing some or all locations)
    /// 3) times for location page (add/edit)x(initial add[not edit]/redo location/redo how use)x(first time/subsequent 'add location' loop/after removing some or all locations)
    /// 4) service details page (when in person, 0 locations) (add/edit)
    ///
    /// or from the 'create location' mini journey (as part of any of the above scenarios)
    /// or after redirecting to self, due to not entering a location (as part of any of the previous scenarios)
    ///
    /// The following logic isn't perfect,
    /// but it's probably good enough without going over the top on complexity for some edge cases.
    /// </summary>
    protected override string GenerateBackUrl()
    {
        // get an optional ServiceJourneyPage from the query params:
        // passed from the service details page when in person and 0 locations
        // passed from the 'locations at service' page

        ServiceJourneyPage? backPage = BackParam;
        if (backPage == null)
        {
            if (Flow == JourneyFlow.Edit ||
                (Flow == JourneyFlow.Add && ChangeFlow != null))
            {
                backPage = ServiceJourneyPage.Locations_For_Service;
            }
            else
            {
                backPage = ServiceJourneyPage.Add_Location;
            }
        }

        return GetServicePageUrl(backPage.Value);
    }

    protected override async Task OnGetWithModelAsync(CancellationToken cancellationToken)
    {
        await PopulateLocationsAndSubstitutions(cancellationToken);

        await UpdateCurrentLocationIfLocationJustAdded(cancellationToken);

        SelectedValue = ServiceModel!.CurrentLocation?.Id.ToString();
    }

    private long? _newlyCreatedLocationId = -1;
    private long? NewlyCreatedLocationId
    {
        get
        {
            if (_newlyCreatedLocationId != -1)
            {
                return _newlyCreatedLocationId;
            }

            _newlyCreatedLocationId = long.TryParse(Request.Query["locationId"], out var newlyCreatedLocationId)
                ? newlyCreatedLocationId
                : null;

            return _newlyCreatedLocationId;
        }
    }

    private async Task UpdateCurrentLocationIfLocationJustAdded(CancellationToken cancellationToken)
    {
        long? locationId = NewlyCreatedLocationId;
        if (locationId == null)
        {
            return;
        }

        var location = await _serviceDirectoryClient.GetLocationById(locationId.Value, cancellationToken);
        ServiceModel!.CurrentLocation = new ServiceLocationModel(location);

        // we need to save to cache now, otherwise we lose the current location if the user hits back
        await Cache.SetAsync(FamilyHubsUser.Email, ServiceModel);
    }

    private async Task PopulateLocationsAndSubstitutions(CancellationToken cancellationToken)
    {
        const string searchName = "";

        string? organisationType = null;

        IEnumerable<LocationDto> locations;
        if (FamilyHubsUser.Role == RoleTypes.DfeAdmin)
        {
            locations = await GetAllLocations(searchName, cancellationToken);
        }
        else
        {
            long organisationId = long.Parse(FamilyHubsUser.OrganisationId);

            locations = await GetLocationsByOrganisation(searchName, organisationId, cancellationToken);

            organisationType = FamilyHubsUser.Role is RoleTypes.LaProfessional or RoleTypes.LaDualRole
                ? "local authority"
                : "organisation";
        }

        locations = RemoveExistingLocations(locations);

        Options = locations
            .Select(l => new SingleAutocompleteOption(l.Id.ToString(), string.Join(", ", l.GetAddress())))
            .OrderBy(o => o.Value);

        ContentSubstitutions = new Dictionary<string, HtmlString>()
        {
            { "OrganisationType", new HtmlString(organisationType) },
            { "AddLocationHref", new HtmlString($"/manage-locations/start-add-location?journey={Journey.Service}&parentJourneyContext={Flow}-{ChangeFlow}") }
        };
    }

    private IEnumerable<LocationDto> RemoveExistingLocations(IEnumerable<LocationDto> locations)
    {
        // we don't remove the current location, as we need to preselect it
        var existingLocationIds = ServiceModel!.Locations
            .Select(l => l.Id)
            .ToHashSet();

        return locations
            .Where(l => !existingLocationIds.Contains(l.Id));
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
        //todo:  BUG - after adding a location, and come back to this page, location is preselected. if user clears input box, then preselected location is used, rather than getting an error message

        if (!long.TryParse(SelectedValue, out var locationId) || locationId == NoSelectionId)
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