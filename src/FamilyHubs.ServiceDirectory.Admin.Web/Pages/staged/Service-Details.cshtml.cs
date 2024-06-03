using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Authorization;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.staged;

[Authorize(Roles = RoleGroups.AdminRole)]
public class Service_DetailsModel : HeaderPageModel
{
    private readonly IServiceDirectoryClient _serviceDirectoryClient;
    private readonly IAiClient _aiClient;

    public Service_DetailsModel(
        IServiceDirectoryClient serviceDirectoryClient,
        IAiClient aiClient)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
        _aiClient = aiClient;
    }

    public async Task OnGetAsync(
        CancellationToken cancellationToken,
        long serviceId)
    {
        var service = await _serviceDirectoryClient.GetServiceById(serviceId, cancellationToken);

        if (!string.IsNullOrEmpty(service.Description))
        {
            var aiCheck = await _aiClient.Call(service.Description, cancellationToken);
        }
    }
}