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

    protected override async Task OnGetWithModelAsync(CancellationToken cancellationToken)
    {
        //todo: document JourneyFlow
        //todo: this will work in 3 modes:
        // 1) adding a service (creating from scratch), retrieving from and setting in the cache
        // 2) redoing when adding a service
        // 3) editing a service, retrieving from and setting in the API

        var service = await _serviceDirectoryClient.GetServiceById(ServiceId, cancellationToken);

        TextBoxValue = service.Name;
    }

    protected override async Task<IActionResult> OnPostWithModelAsync(CancellationToken cancellationToken)
    {
        //todo: PRG
        if (string.IsNullOrWhiteSpace(TextBoxValue))
        {
            return RedirectToSelf(null, ErrorId.Service_Name__EnterNameOfService);
        }

        var service = await _serviceDirectoryClient.GetServiceById(ServiceId, cancellationToken);
        service.Name = TextBoxValue!;
        await _serviceDirectoryClient.UpdateService(service, cancellationToken);

        return NextPage();
    }
}