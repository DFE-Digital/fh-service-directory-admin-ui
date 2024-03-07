//using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
//using FamilyHubs.ServiceDirectory.Admin.Core.Models;
//using FamilyHubs.ServiceDirectory.Admin.Web.Journeys;
//using FamilyHubs.SharedKernel.Identity;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;

//namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

//[Authorize(Roles = RoleGroups.AdminRole)]
//public class location_redirectModel : PageModel
//{
//    private readonly IRequestDistributedCache _cache;

//    public location_redirectModel(
//        IRequestDistributedCache cache)
//    {
//        _cache = cache;
//    }

//    public async Task<IActionResult> OnGetAsync(long? locationId, string? operation)
//    {
//        ArgumentNullException.ThrowIfNull(locationId);
//        ArgumentNullException.ThrowIfNull(operation);

//        var familyHubsUser = HttpContext.GetFamilyHubsUser();

//        var serviceModel = await _cache.GetAsync<ServiceModel<object>>(familyHubsUser.Email);
//        if (serviceModel == null)
//        {
//            // the journey cache entry has expired, and we don't have a model to work with
//            // likely the user has come back to this page after a long time
//            return Redirect("/Welcome"); //GetServicePageUrl(ServiceJourneyPage.Initiator, Flow));
//        }

//        if ((serviceModel.Locations.Single(l => l.Id == locationId)) == null)
//        {
//            throw new InvalidOperationException("Location not associated with service");
//        }

//        serviceModel.OperationLocationId = locationId;

//        return Redirect();
//    }
//}