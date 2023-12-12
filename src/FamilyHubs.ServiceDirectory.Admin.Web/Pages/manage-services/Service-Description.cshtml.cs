using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;


[Authorize(Roles = RoleGroups.AdminRole)]
public class Service_DescriptionModel : ServicePageModel
{

    public string TextBoxLabel { get; set; } = "Give a description of the service";
    public int? MaxLength => 200;

    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    [BindProperty]
    public string? TextBoxValue { get; set; }

    public Service_DescriptionModel(
        IRequestDistributedCache connectionRequestCache,
        IServiceDirectoryClient serviceDirectoryClient)
    : base(ServiceJourneyPage.Service_Description, connectionRequestCache)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    protected override async Task OnGetWithModelAsync(CancellationToken cancellationToken)
    {
        if (Errors.HasErrors)
        {
            return;
        }

        switch (Flow)
        {
            case JourneyFlow.Edit:
                var service = await _serviceDirectoryClient.GetServiceById(ServiceId!.Value, cancellationToken);

                TextBoxValue = service.Description;
                break;

            default:
                //todo: make ServiceModel non-nullable (either change back to passing (and make model? private), or non-nullable and default?)
                TextBoxValue = ServiceModel!.Description;
                break;
        }
    }

    protected override async Task<IActionResult> OnPostWithModelAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(TextBoxValue))
        {
            return RedirectToSelf(ErrorId.Service_Description__EnterDescriptionOfService);
        }

        if (TextBoxValue.Length > MaxLength)
        {
            TextBoxValue = TextBoxValue[..MaxLength.Value];
        }

        switch (Flow)
        {
            case JourneyFlow.Edit:
                await UpdateServiceDescription(TextBoxValue!, cancellationToken);
                break;
            default:
                ServiceModel!.Description = TextBoxValue;
                break;
        }

        return NextPage();
    }

    private async Task UpdateServiceDescription(string serviceDescription, CancellationToken cancellationToken)
    {
        var service = await _serviceDirectoryClient.GetServiceById(ServiceId!.Value, cancellationToken);
        service.Description = serviceDescription;
        await _serviceDirectoryClient.UpdateService(service, cancellationToken);
    }
}