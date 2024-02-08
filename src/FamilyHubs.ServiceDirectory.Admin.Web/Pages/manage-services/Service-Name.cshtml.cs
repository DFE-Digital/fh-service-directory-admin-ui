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
        //todo: make ServiceModel non-nullable (either change back to passing (and make model? private), or non-nullable and default?)
        TextBoxValue = ServiceModel!.Name;
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

        ServiceModel!.Updated = ServiceModel.Updated || HasNameBeenUpdated();

        ServiceModel!.Name = TextBoxValue;

        return NextPage();
    }

    private bool HasNameBeenUpdated()
    {
        return ServiceModel!.Name != TextBoxValue;
    }
}