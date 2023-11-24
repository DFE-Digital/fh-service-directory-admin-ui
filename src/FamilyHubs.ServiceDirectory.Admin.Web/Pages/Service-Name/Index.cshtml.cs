using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Errors;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Razor.ErrorNext;
using FamilyHubs.SharedKernel.Razor.FullPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.Service_Name;

[Authorize(Roles = RoleGroups.AdminRole)]
public class IndexModel : ServiceWithCachePageModel, ISingleTextboxPageModel
{
    public string? HeadingText { get; set; }
    public string? HintText { get; set; }
    public string TextBoxLabel { get; set; } = "What is the service name?";

    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    [BindProperty]
    public string? TextBoxValue { get; set; }

    public IndexModel(
        IRequestDistributedCache connectionRequestCache,
        IServiceDirectoryClient serviceDirectoryClient)
    : base(ServiceJourneyPage.Service_Name, connectionRequestCache)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    public async Task OnGetAsync(long? serviceId, CancellationToken cancellationToken)
    {
        //todo: this will work in 2 modes: as part of creating a service, where it'll have to support fetching a current service from the cache

        ArgumentNullException.ThrowIfNull(serviceId);

        var service = await _serviceDirectoryClient.GetServiceById(serviceId.Value, cancellationToken);

        TextBoxValue = service.Name;
    }

    public async Task<IActionResult> OnPostAsync(long? serviceId)
    {
        ArgumentNullException.ThrowIfNull(serviceId);

        //todo: PRG
        if (string.IsNullOrWhiteSpace(TextBoxValue))
        {
            return RedirectToSelf(null, ErrorId.Service_Name__EnterNameOfService);
        }

        var service = await _serviceDirectoryClient.GetServiceById(serviceId.Value);
        service.Name = TextBoxValue!;
        await _serviceDirectoryClient.UpdateService(service);

        return RedirectToPage("/manage-services/service-detail", new { id = serviceId });
    }
}