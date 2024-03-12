using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Models.LocationJourney;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_locations;

[Authorize(Roles = RoleGroups.AdminRole)]
public class Location_DetailsModel : LocationPageModel
{
    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public Location_DetailsModel(
        IRequestDistributedCache connectionRequestCache,
        IServiceDirectoryClient serviceDirectoryClient)
        : base(LocationJourneyPage.Location_Details, connectionRequestCache)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    public IEnumerable<string> GetAddress()
    {
        return RemoveEmpty(
            LocationModel!.Name,
            LocationModel.AddressLine1,
            LocationModel.AddressLine2,
            LocationModel.City,
            LocationModel.County,
            LocationModel.Postcode);
    }

    private static IEnumerable<string> RemoveEmpty(params string?[] list)
    {
        return list.Where(x => !string.IsNullOrWhiteSpace(x))!;
    }

    protected override async Task<IActionResult> OnPostWithModelAsync(CancellationToken cancellationToken)
    {
        if (Flow == JourneyFlow.Edit)
        {
            await UpdateLocation(cancellationToken);
            return RedirectToPage("/manage-locations/Location-Saved-Confirmation");
        }

        var newLocationId = await AddLocation(cancellationToken);

        if (Journey == Journey.Service)
        {
            return RedirectToPage("/manage-services/Select-Location", new { flow = "add" , locationId = newLocationId});
        }
        //todo: if Journey is Location, we need to send them back to the location journey, but do we show them the confirmation first?
        //probably better to have a continue button on the confirmation page, but need to check story
        return RedirectToPage("/manage-locations/LocationAddedConfirmation");
    }

    private async Task<long> AddLocation(CancellationToken cancellationToken)
    {
        var location = new LocationDto
        {
            LocationTypeCategory = (LocationModel!.IsFamilyHub == true)
                ? LocationTypeCategory.FamilyHub : LocationTypeCategory.NotSet,
            Description = LocationModel.Description,
            Name = LocationModel.Name ?? "",
            Address1 = LocationModel.AddressLine1!,
            Address2 = LocationModel.AddressLine2 ?? "",
            City = LocationModel.City!,
            StateProvince = LocationModel.County ?? "",
            PostCode = LocationModel.Postcode!,
            Country = "GB",
            Latitude = LocationModel.Latitude!.Value,
            Longitude = LocationModel.Longitude!.Value,
            LocationType = LocationType.Postal,
            OrganisationId = LocationModel.OrganisationId
        };

        //todo: if the user tries to add a duplicate location, we should report that with a friendly message
        // rather than a service error

        return await _serviceDirectoryClient.CreateLocation(location, cancellationToken);
    }

    private async Task UpdateLocation(CancellationToken cancellationToken)
    {
        long locationId = LocationModel!.Id!.Value;
        var location = await _serviceDirectoryClient.GetLocationById(locationId, cancellationToken);
        if (location is null)
        {
            //todo: better exception?
            throw new InvalidOperationException($"Location not found: {locationId}");
        }

        UpdateLocationFromCache(location);

        await _serviceDirectoryClient.UpdateLocation(location, cancellationToken);
    }

    private void UpdateLocationFromCache(LocationDto location)
    {
        location.LocationTypeCategory = LocationModel!.IsFamilyHub!.Value ? LocationTypeCategory.FamilyHub : LocationTypeCategory.NotSet;
        location.Description = LocationModel.Description;
        location.Name = LocationModel.Name ?? "";
        location.Address1 = LocationModel.AddressLine1!;
        location.Address2 = LocationModel.AddressLine2 ?? "";
        location.City = LocationModel.City!;
        location.StateProvince = LocationModel.County ?? "";
        location.PostCode = LocationModel.Postcode!;
        location.Country = "GB";
        location.Latitude = LocationModel.Latitude!.Value;
        location.Longitude = LocationModel.Longitude!.Value;
        location.LocationType = LocationType.Postal;
        location.OrganisationId = LocationModel.OrganisationId;
    }
}