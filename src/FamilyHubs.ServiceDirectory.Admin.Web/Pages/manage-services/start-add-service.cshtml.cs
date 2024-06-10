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

        var serviceModel = new ServiceModel
        {
            ServiceType = GetServiceTypeArg(serviceType)
        };

        if (familyHubsUser.Role != RoleTypes.DfeAdmin)
        {
            serviceModel.OrganisationId = long.Parse(familyHubsUser.OrganisationId);
        }

        // the user's just starting the journey
        await _cache.SetAsync(familyHubsUser.Email, serviceModel);

        return Redirect(ServiceJourneyPageExtensions.GetAddFlowStartPagePath(familyHubsUser.Role));
    }

    private ServiceTypeArg GetServiceTypeArg(string? serviceType)
    {
        if (!Enum.TryParse<ServiceTypeArg>(serviceType, out var serviceTypeEnum))
        {
            // it's only really needed for the dfe admin, but we'll require it for consistency (and for when we allow LAs to add VCS services)
            throw new InvalidOperationException("ServiceType must be passed as a query parameter");
        }
        return serviceTypeEnum;
    }
}