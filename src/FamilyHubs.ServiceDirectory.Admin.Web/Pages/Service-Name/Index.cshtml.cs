using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Identity;
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

        switch (Flow)
        {
            case JourneyFlow.Edit:
                //todo: best way to say is non-null when JourneyFlow.Edit?
                var service = await _serviceDirectoryClient.GetServiceById(ServiceId!.Value, cancellationToken);

                TextBoxValue = service.Name;
                break;

            default:
                //todo: make ServiceModel non-nullable (either change back to passing (and make model? private), or non-nullable and default?)
                TextBoxValue = ServiceModel!.Name;
                break;
        }
    }

    protected override async Task<IActionResult> OnPostWithModelAsync(CancellationToken cancellationToken)
    {
        //todo: PRG
        if (string.IsNullOrWhiteSpace(TextBoxValue))
        {
            return RedirectToSelf(null, ErrorId.Service_Name__EnterNameOfService);
        }

        //todo: truncate if too long

        switch (Flow)
        {
            case JourneyFlow.Edit:
                await UpdateServiceName(TextBoxValue!, cancellationToken);
                break;
            default:
                ServiceModel!.Name = TextBoxValue;
                break;
        }

        return NextPage();
    }

    private async Task UpdateServiceName(string serviceName, CancellationToken cancellationToken)
    {
        var service = await _serviceDirectoryClient.GetServiceById(ServiceId!.Value, cancellationToken);
        service.Name = serviceName;
        await _serviceDirectoryClient.UpdateService(service, cancellationToken);
    }
}