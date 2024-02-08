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

        //todo: make ServiceModel non-nullable (either change back to passing (and make model? private), or non-nullable and default?)
        TextAreaValue = ServiceModel!.Description;
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

        ServiceModel!.Description = TextAreaValue;

        return NextPage();
    }
}