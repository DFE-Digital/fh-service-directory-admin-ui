using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Razor.FullPages.SingleTextArea;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class Service_DescriptionModel : ServicePageModel<string?>, ISingleTextAreaPageModel
{
    [BindProperty]
    public string? TextAreaValue { get; set; }

    public string DescriptionPartial => "Service-Description-Content";
    public string? Label => null;
    public int TextAreaMaxLength => 200;
    public int TextAreaNumberOfRows => 4;

    private readonly IServiceDirectoryClient _serviceDirectoryClient;

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
            TextAreaValue = ServiceModel!.UserInput;
            return;
        }

        switch (Flow)
        {
            case JourneyFlow.Edit:
                var service = await _serviceDirectoryClient.GetServiceById(ServiceId!.Value, cancellationToken);

                TextAreaValue = service.Description;
                break;

            default:
                //todo: make ServiceModel non-nullable (either change back to passing (and make model? private), or non-nullable and default?)
                TextAreaValue = ServiceModel!.Description;
                break;
        }
    }

    protected override async Task<IActionResult> OnPostWithModelAsync(CancellationToken cancellationToken)
    {
        var errorId = this.CheckForErrors(
            ErrorId.Service_Description__EnterDescriptionOfService,
            ErrorId.Service_Description__TooLong);

        if (errorId != null)
        {
            //todo: need to truncate the user input to something sensible
            return RedirectToSelf(TextAreaValue, errorId.Value);
        }

        switch (Flow)
        {
            case JourneyFlow.Edit:
                await UpdateServiceDescription(TextAreaValue!, cancellationToken);
                break;
            default:
                ServiceModel!.Description = TextAreaValue;
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