using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Razor.FullPages.SingleTextbox;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

//todo: Remove our cache on sign-out
[Authorize(Roles = RoleGroups.AdminRole)]
public class Service_NameModel : ServicePageModel, ISingleTextboxPageModel
{
    public string? HeadingText { get; set; }
    public string? HintText { get; set; }
    public string TextBoxLabel { get; set; } = "What is the service name?";
    public int? MaxLength => 255;

    [BindProperty]
    public string? TextBoxValue { get; set; }

    public Service_NameModel(IRequestDistributedCache connectionRequestCache)
    : base(ServiceJourneyPage.Service_Name, connectionRequestCache)
    {
    }

    protected override void OnGetWithModel()
    {
        if (Errors.HasErrors)
        {
            return;
        }

        //switch (Flow)
        //{
        //    case JourneyFlow.Edit:
        //        var service = await _serviceDirectoryClient.GetServiceById(ServiceId!.Value, cancellationToken);

        //        TextBoxValue = service.Name;
        //        break;

        //    default:
                //todo: make ServiceModel non-nullable (either change back to passing (and make model? private), or non-nullable and default?)
                TextBoxValue = ServiceModel!.Name;
        //        break;
        //}
    }

    protected override IActionResult OnPostWithModel()
    {
        if (string.IsNullOrWhiteSpace(TextBoxValue))
        {
            return RedirectToSelf(ErrorId.Service_Name__EnterNameOfService);
        }

        if (TextBoxValue.Length > MaxLength)
        {
            TextBoxValue = TextBoxValue[..MaxLength.Value];
        }

        //switch (Flow)
        //{
        //    case JourneyFlow.Edit:
        //        await UpdateServiceName(TextBoxValue!, cancellationToken);
        //        break;
        //    default:
                ServiceModel!.Name = TextBoxValue;
        //        break;
        //}

        return NextPage();
    }

    //private async Task UpdateServiceName(string serviceName, CancellationToken cancellationToken)
    //{
    //    var service = await _serviceDirectoryClient.GetServiceById(ServiceId!.Value, cancellationToken);
    //    service.Name = serviceName;
    //    await _serviceDirectoryClient.UpdateService(service, cancellationToken);
    //}
}