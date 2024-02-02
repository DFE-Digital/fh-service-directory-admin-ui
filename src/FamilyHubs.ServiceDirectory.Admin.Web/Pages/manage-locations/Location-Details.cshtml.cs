using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Services.Postcode.Interfaces;
using FamilyHubs.SharedKernel.Services.Postcode.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Design;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_locations;

[Authorize(Roles = RoleGroups.AdminRole)]
public class Location_DetailsModel : LocationPageModel
{
    private readonly IServiceDirectoryClient _serviceDirectoryClient;
    private readonly IPostcodeLookup _postcodeLookup;

    public Location_DetailsModel(
        IRequestDistributedCache connectionRequestCache,
        IServiceDirectoryClient serviceDirectoryClient,
        IPostcodeLookup postcodeLookup)
        : base(LocationJourneyPage.Location_Details, connectionRequestCache)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
        _postcodeLookup = postcodeLookup;
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
        var postcodeInfo = await GetPostcodeInfo(LocationModel!.Postcode!, cancellationToken);

        if (Flow == JourneyFlow.Edit)
        {
            await UpdateLocation(postcodeInfo, cancellationToken);
            return RedirectToPage("/manage-locations/Location-Saved-Confirmation");
        }

        await AddLocation(postcodeInfo, cancellationToken);
        return RedirectToPage("/manage-locations/LocationAddedConfirmation");
    }

    private async Task<IPostcodeInfo> GetPostcodeInfo(string postcode, CancellationToken cancellationToken)
    {
        var (postcodeError, postcodeInfo) = await _postcodeLookup.Get(postcode, cancellationToken);
        if (postcodeError != PostcodeError.None)
        {
            //todo: need user error messages, probably on the address page if postcode is not found (or invalid)
            throw new OperationException($"Issue with postcode: {postcodeError}");
        }

        return postcodeInfo!;
    }

    private async Task AddLocation(IPostcodeInfo postcodeInfo, CancellationToken cancellationToken)
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
            //todo: better for API to add this?
            Latitude = postcodeInfo.Latitude!.Value,
            Longitude = postcodeInfo.Longitude!.Value
        };

        await _serviceDirectoryClient.CreateLocation(location, cancellationToken);
    }

    private async Task UpdateLocation(IPostcodeInfo postcodeInfo, CancellationToken cancellationToken)
    {
        long locationId = LocationModel!.Id!.Value;
        var location = await _serviceDirectoryClient.GetLocationById(locationId, cancellationToken);
        if (location is null)
        {
            //todo: better exception?
            throw new InvalidOperationException($"Location not found: {locationId}");
        }

        UpdateLocationFromCache(location, postcodeInfo);

        await _serviceDirectoryClient.UpdateLocation(location, cancellationToken);
    }

    private void UpdateLocationFromCache(LocationDto location, IPostcodeInfo postcodeInfo)
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
        //todo: better for API to add this?
        location.Latitude = postcodeInfo.Latitude!.Value;
        location.Longitude = postcodeInfo.Longitude!.Value;
    }
}