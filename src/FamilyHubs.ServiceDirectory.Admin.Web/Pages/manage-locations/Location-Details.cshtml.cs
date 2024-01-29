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

    protected override async Task<IActionResult> OnPostWithModelAsync(CancellationToken cancellationToken)
    {
        await AddLocation(cancellationToken);

        return RedirectToPage("/manage-locations/Confirmation");
    }

    private async Task AddLocation(CancellationToken cancellationToken)
    {
        var location = new LocationDto
        {
            LocationType = LocationModel!.IsFamilyHub!.Value ? LocationType.FamilyHub : LocationType.NotSet,
            Description = LocationModel.Description,
            Name = LocationModel.BuildingName ?? "",
            Address1 = LocationModel.Line1!,
            Address2 = LocationModel.Line2 ?? "",
            City = LocationModel.TownOrCity!,
            StateProvince = "",
            PostCode = LocationModel.Postcode!,
            Country = "England",
            //todo: better for API to add this?
            Latitude = 0,
            Longitude = 0
        };

        await _serviceDirectoryClient.CreateLocation(location, cancellationToken);
    }
}