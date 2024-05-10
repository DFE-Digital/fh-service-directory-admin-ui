using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Models.ServiceJourney;
using FamilyHubs.ServiceDirectory.Admin.Web.Journeys;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class start_add_serviceModel : PageModel
{
    private readonly IRequestDistributedCache _cache;

    public start_add_serviceModel(IRequestDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<IActionResult> OnGetAsync(string? serviceType)
    {
        var familyHubsUser = HttpContext.GetFamilyHubsUser();

        var serviceModel = new ServiceModel();

        if (familyHubsUser.Role == RoleTypes.DfeAdmin)
        {
            //if (!Enum.TryParse<ServiceType>(serviceType, out var serviceTypeEnum))
            //{
            //    throw new InvalidOperationException("When adding a service as a DfE admin, ServiceType must be passed as a query parameter");
            //}
            //serviceModel.ServiceType = serviceTypeEnum;

            //todo: use OrganisationType, or existing ServiceType?
            //todo: will probably have to set this in start-edit-service too
            serviceModel.ServiceType = serviceType.ToEnum<ServiceTypeArg>();
        }

        // the user's just starting the journey
        await _cache.SetAsync(familyHubsUser.Email, new ServiceModel());

        return Redirect(ServiceJourneyPageExtensions.GetAddFlowStartPagePath());
    }
}