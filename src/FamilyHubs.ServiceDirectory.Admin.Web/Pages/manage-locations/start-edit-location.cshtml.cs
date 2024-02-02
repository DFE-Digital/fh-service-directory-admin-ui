using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_locations;

public class start_edit_locationModel : PageModel
{
    private readonly IRequestDistributedCache _cache;
    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public start_edit_locationModel(
        IRequestDistributedCache cache,
        IServiceDirectoryClient serviceDirectoryClient)
    {
        _cache = cache;
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    public async Task<IActionResult> OnGetAsync(long? locationId)
    {
        if (locationId == null)
        {
            throw new ArgumentNullException(nameof(locationId));
        }

        var location = await _serviceDirectoryClient.GetLocationById(locationId.Value);

        var familyHubsUser = HttpContext.GetFamilyHubsUser();

        // the user's just starting the journey
        await _cache.SetAsync(familyHubsUser.Email, CreateLocationModel(locationId.Value, location));

        return Redirect(LocationJourneyPageExtensions.GetEditStartPagePath());
    }

    private LocationModel CreateLocationModel(long locationId, LocationDto location)
    {
        return new LocationModel
        {
            Id = locationId,
            Name = location.Name,
            AddressLine1 = location.Address1,
            AddressLine2 = location.Address2,
            City = location.City,
            County = location.StateProvince,
            Postcode = location.PostCode,
            IsFamilyHub = location.LocationType == LocationType.FamilyHub,
            Description = location.Description
        };
    }
}