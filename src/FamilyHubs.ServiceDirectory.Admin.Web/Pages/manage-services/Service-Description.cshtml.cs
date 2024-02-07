using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Razor.FullPages.SingleTextArea;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

//todo: issue when editing location & service : go to a page, get error message, go back to details page, go to a different page -> json deserialization error as userinput from old page still in error cache
// need to clear down error cache on details page??

public class Service_DescriptionModel : ServicePageModel<string?>, ISingleTextAreaPageModel
{
    [BindProperty]
    public string? TextAreaValue { get; set; }

    public string DescriptionPartial => "Service-Description-Content";
    public string? Label => null;
    public int TextAreaMaxLength => 200;
    public int TextAreaNumberOfRows => 4;

    public Service_DescriptionModel(IRequestDistributedCache connectionRequestCache)
    : base(ServiceJourneyPage.Service_Description, connectionRequestCache)
    {
    }

    protected override void OnGetWithModel()
    {
        if (Errors.HasErrors)
        {
            TextAreaValue = ServiceModel!.UserInput;
            return;
        }

        //switch (Flow)
        //{
        //    case JourneyFlow.Edit:
        //        var service = await _serviceDirectoryClient.GetServiceById(ServiceId!.Value, cancellationToken);

        //        TextAreaValue = service.Description;
        //        break;

        //    default:
                //todo: make ServiceModel non-nullable (either change back to passing (and make model? private), or non-nullable and default?)
                TextAreaValue = ServiceModel!.Description;
        //        break;
        //}
    }

    protected override IActionResult OnPostWithModel()
    {
        var errorId = this.CheckForErrors(
            ErrorId.Service_Description__EnterDescriptionOfService,
            ErrorId.Service_Description__TooLong);

        if (errorId != null)
        {
            //todo: need to truncate the user input to something sensible
            return RedirectToSelf(TextAreaValue, errorId.Value);
        }

        //switch (Flow)
        //{
        //    case JourneyFlow.Edit:
        //        await UpdateServiceDescription(TextAreaValue!, cancellationToken);
        //        break;
        //    default:
                ServiceModel!.Description = TextAreaValue;
        //        break;
        //}

        return NextPage();
    }

    //private async Task UpdateServiceDescription(string serviceDescription, CancellationToken cancellationToken)
    //{
    //    var service = await _serviceDirectoryClient.GetServiceById(ServiceId!.Value, cancellationToken);
    //    service.Description = serviceDescription;
    //    await _serviceDirectoryClient.UpdateService(service, cancellationToken);
    //}
}