using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class start_edit_serviceModel : PageModel
{
    private readonly IRequestDistributedCache _cache;
    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public start_edit_serviceModel(
        IRequestDistributedCache cache,
        IServiceDirectoryClient serviceDirectoryClient)
    {
        _cache = cache;
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    public async Task<IActionResult> OnGetAsync(long? serviceId)
    {
        if (serviceId == null)
        {
            throw new ArgumentNullException(nameof(serviceId));
        }

        var service = await _serviceDirectoryClient.GetServiceById(serviceId.Value);

        var familyHubsUser = HttpContext.GetFamilyHubsUser();

        // the user's just starting the journey
        await _cache.SetAsync(familyHubsUser.Email, CreateServiceModel(serviceId.Value, service));

        return Redirect(ServiceJourneyPageExtensions.GetEditStartPagePath());
    }

    private ServiceModel CreateServiceModel(long serviceId, ServiceDto service)
    {
        var serviceModel = new ServiceModel
        {
            Id = serviceId,
            Name = service.Name,
            Description = service.Description
        };

        //todo: extract
        var eligibility = service.Eligibilities.FirstOrDefault();
        serviceModel.ForChildren = eligibility != null;
        if (serviceModel.ForChildren == true)
        {
            serviceModel.MinimumAge = eligibility!.MinimumAge;
            serviceModel.MaximumAge = eligibility.MaximumAge;
        }

        //todo: other properties

        return serviceModel;
    }
}