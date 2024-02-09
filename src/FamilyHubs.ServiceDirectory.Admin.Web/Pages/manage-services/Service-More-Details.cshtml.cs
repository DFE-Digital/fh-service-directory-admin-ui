using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Razor.FullPages.SingleTextArea;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class Service_MoreDetailsModel : ServicePageModel<string?>, ISingleTextAreaPageModel
{
    [BindProperty]
    public string? TextAreaValue { get; set; }

    public string DescriptionPartial => "Service-More-Details-Content";
    public string? Label => null;
    public int TextAreaMaxLength => 500;
    public int TextAreaNumberOfRows => 6;

    public Service_MoreDetailsModel(IRequestDistributedCache connectionRequestCache)
    : base(ServiceJourneyPage.Service_More_Details, connectionRequestCache)
    {
    }

    protected override void OnGetWithError()
    {
        TextAreaValue = ServiceModel!.UserInput;
    }

    protected override void OnGetWithModel()
    {
        //todo: make ServiceModel non-nullable (either change back to passing (and make model? private), or non-nullable and default?)
        TextAreaValue = ServiceModel!.MoreDetails;
    }

    protected override IActionResult OnPostWithModel()
    {
        var errorId = this.CheckForErrors(ErrorId.Service_More_Details__TooLong);

        if (errorId != null)
        {
            //todo: need to truncate the user input to something sensible
            return RedirectToSelf(TextAreaValue, errorId.Value);
        }

        ServiceModel!.Updated = ServiceModel.Updated || HasMoreDetailsBeenUpdated();

        ServiceModel!.MoreDetails = TextAreaValue;

        return NextPage();
    }

    private bool HasMoreDetailsBeenUpdated()
    {
        return ServiceModel!.MoreDetails != TextAreaValue;
    }
}