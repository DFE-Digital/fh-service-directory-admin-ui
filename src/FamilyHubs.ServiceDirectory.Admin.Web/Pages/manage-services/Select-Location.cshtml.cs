using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Display;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class Select_LocationModel : ServicePageModel
{
    public const int NoSelectionLocationId = -1;
    public long? SelectedLocationId { get; private set; }
    public IEnumerable<LocationDto> Locations { get; private set; } = Enumerable.Empty<LocationDto>();
    public string? OrganisationName { get; private set; }

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

        var locationIdString = Request.Query["locationId"];
        if (!string.IsNullOrEmpty(locationIdString))
        {
            long locationId = long.Parse(locationIdString!);
            if (locationId > 0)
            {
                SelectedLocationId = locationId;
                return;
            }
        }

        SelectedLocationId = ServiceModel!.CurrentLocation;
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
            var locationsTask = GetLocationsByOrganisation(searchName, organisationId, cancellationToken);

            var organisationNameTask = GetOrganisationName(organisationId, cancellationToken);

            await Task.WhenAll(locationsTask, organisationNameTask);

            Locations = locationsTask.Result;
            OrganisationName = organisationNameTask.Result;
        }

        foreach (var location in Locations)
        {
            // 'borrow' the description field to store the address
            location.Description = string.Join(", ", location.GetAddress());
        }

        Locations = Locations
            .OrderBy(l => l.Description);
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

    protected override IActionResult OnPostWithModel()
    {
        string? locationIdString = Request.Form["location"];

        long locationId = long.Parse(locationIdString);

        if (locationId == NoSelectionLocationId)
        {
            return RedirectToSelf(ErrorId.Select_Location__NoLocationSelected);
        }

        ServiceModel!.CurrentLocation = locationId;

        return NextPage();
    }
}