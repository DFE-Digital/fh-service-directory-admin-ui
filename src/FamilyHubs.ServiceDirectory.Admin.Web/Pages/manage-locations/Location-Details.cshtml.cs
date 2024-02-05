using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
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

        await AddLocation(cancellationToken);
        return RedirectToPage("/manage-locations/LocationAddedConfirmation");
    }

    private async Task AddLocation(CancellationToken cancellationToken)
    {
        var location = new LocationDto
        {
            LocationType = LocationModel!.IsFamilyHub!.Value ? LocationType.FamilyHub : LocationType.NotSet,
            Description = LocationModel.Description,
            Name = LocationModel.Name ?? "",
            Address1 = LocationModel.AddressLine1!,
            Address2 = LocationModel.AddressLine2 ?? "",
            City = LocationModel.City!,
            StateProvince = LocationModel.County ?? "",
            PostCode = LocationModel.Postcode!,
            Country = "UK",
            Latitude = LocationModel.Latitude!.Value,
            Longitude = LocationModel.Longitude!.Value
        };

        //todo: if the user tries to add a duplicate location, we should report that with a friendly message
        // rather than a service error

        await _serviceDirectoryClient.CreateLocation(location, cancellationToken);
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
        location.Id = LocationModel!.Id!.Value;
        location.LocationType = LocationModel!.IsFamilyHub!.Value ? LocationType.FamilyHub : LocationType.NotSet;
        location.Description = LocationModel.Description;
        location.Name = LocationModel.Name ?? "";
        location.Address1 = LocationModel.AddressLine1!;
        location.Address2 = LocationModel.AddressLine2 ?? "";
        location.City = LocationModel.City!;
        location.StateProvince = LocationModel.County ?? "";
        location.PostCode = LocationModel.Postcode!;
        location.Country = "UK";
        location.Latitude = LocationModel.Latitude!.Value;
        location.Longitude = LocationModel.Longitude!.Value;
    }
}