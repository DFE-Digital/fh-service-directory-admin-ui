using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Models.LocationJourney;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Razor.FullPages.SingleTextArea;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_locations;

public class Location_InformationModel : LocationPageModel<string?>, ISingleTextAreaPageModel
{
    [BindProperty]
    public string? TextAreaValue { get; set; }
    public string DescriptionPartial => "Location-Information-Content";
    public string? Label => null;
    public int TextAreaMaxLength => 500;
    public int TextAreaNumberOfRows => 9;

    public Location_InformationModel(IRequestDistributedCache connectionRequestCache)
        : base(LocationJourneyPage.Location_Information, connectionRequestCache)
    {
    }

    protected override void OnGetWithError()
    {
        TextAreaValue = LocationModel!.UserInput;
    }

    protected override void OnGetWithModel()
    {
        TextAreaValue = LocationModel!.Description;
    }

    protected override IActionResult OnPostWithModel()
    {
        var errorId = this.CheckForErrors(ErrorId.Location_Information__TooLong);
        if (errorId != null)
        {
            //todo: need to truncate the user input to something sensible
            return RedirectToSelf(TextAreaValue, errorId.Value);
        }

        LocationModel!.Updated = LocationModel.Updated || HasDescriptionBeenUpdated();

        LocationModel!.Description = TextAreaValue;

        return NextPage();
    }

    private bool HasDescriptionBeenUpdated()
    {
        return LocationModel!.Description != TextAreaValue;
    }
}