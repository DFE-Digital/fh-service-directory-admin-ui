using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Common;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Razor.FullPages.Checkboxes;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public enum HowUse
{
    InPerson,
    Online,
    Telephone
}

public class How_UseModel : ServicePageModel, ICheckboxesPageModel
{
    public static Checkbox[] StaticCheckboxes => new[]
    {
        new Checkbox("In person", HowUse.InPerson.ToString()),
        new Checkbox("Online", HowUse.Online.ToString()),
        new Checkbox("Telephone", HowUse.Telephone.ToString())
    };

    public IEnumerable<ICheckbox> Checkboxes => StaticCheckboxes;

    [BindProperty]
    public IEnumerable<string> SelectedValues { get; set; } = Enumerable.Empty<string>();

    public string? DescriptionPartial => null;
    public string? Legend => "How can people use this service?";
    // have this string as the default?
    public string? Hint => "Select all options that apply.";

    public How_UseModel(IRequestDistributedCache connectionRequestCache)
        : base(ServiceJourneyPage.How_Use, connectionRequestCache)
    {
    }

    protected override void OnGetWithModel()
    {
        //SelectedValues = ServiceModel!.Times ?? Enumerable.Empty<string>();
    }

    protected override IActionResult OnPostWithModel()
    {
        if (!SelectedValues.Any())
        {
            return RedirectToSelf(ErrorId.How_Use__MissingSelection);
        }

        ServiceModel!.Updated = ServiceModel!.Updated || HasHowUseBeenUpdated();

        //ServiceModel!.Times = SelectedValues;

        return NextPage();
    }

    private bool HasHowUseBeenUpdated()
    {
        return false;
        //return ServiceModel!.Times != null &&
        //       !ServiceModel.Times
        //           .OrderBy(x => x)
        //           .SequenceEqual(SelectedValues.OrderBy(x => x));
    }
}