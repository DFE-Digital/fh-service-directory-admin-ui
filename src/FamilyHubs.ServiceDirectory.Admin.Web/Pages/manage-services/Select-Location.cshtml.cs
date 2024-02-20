using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Models;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class Select_LocationModel : ServicePageModel
{
    public IEnumerable<LocationDto> Locations { get; set; } = Enumerable.Empty<LocationDto>();
    public long SelectedLocationId { get; set; }

    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public Select_LocationModel(
        IServiceDirectoryClient serviceDirectoryClient,
        IRequestDistributedCache connectionRequestCache)
        : base(ServiceJourneyPage.Select_Location, connectionRequestCache)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    protected override async Task OnGetWithModelAsync(CancellationToken cancellationToken)
    {
        //todo: PRG and pass the search name through user input
        string searchName = "";

        long organisationId = long.Parse(FamilyHubsUser.OrganisationId);

        //todo: should have a single PaginatedList?
        var locations = GetLocations(searchName, organisationId, cancellationToken);

        var organisationName = GetOrganisationName(organisationId, cancellationToken);

        await Task.WhenAll(locations, organisationName);

        Locations = locations.Result;
    }

    private async Task<string> GetOrganisationName(long organisationId, CancellationToken cancellationToken)
    {
        var organisation = await _serviceDirectoryClient.GetOrganisationById(organisationId, cancellationToken);
        return organisation.Name;
    }

    private async Task<List<LocationDto>> GetLocations(
        string searchName,
        long organisationId,
        CancellationToken cancellationToken)
    {
        // we ask for a maximum of 10000, as the front end is limited to 10000 anyway, see https://chromium.googlesource.com/chromium/blink.git/+/master/Source/core/html/HTMLSelectElement.cpp#77
        const int maxLocations = 10000;

        //todo: should have a single PaginatedList?
        PaginatedList<LocationDto> locations;

        if (FamilyHubsUser.Role == RoleTypes.DfeAdmin)
        {
            //todo: as an optimisation, could have a version without sorting etc.
            //todo: some of these are mandatory in the client, but not in the api - refactor params
            // passing "" as orderbyColumn should mean no ordering is done, which is ideal for us

            locations = await _serviceDirectoryClient.GetLocations(true, "", searchName, false, 1, maxLocations, cancellationToken);
        }
        else
        {
            locations = await _serviceDirectoryClient.GetLocationsByOrganisationId(organisationId, null, "", searchName, false, 1, maxLocations, cancellationToken);
        }

        return locations.Items;
    }

    protected override IActionResult OnPostWithModel()
    {
        return NextPage();
    }
}