using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Models.ServiceJourney;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class Time_Details_At_LocationModel : ServicePageModel<TimeDetailsUserInput>
{
    public string TextBoxLabel { get; set; } = "Can you provide more details about using this service [at location]?";
    public int? MaxLength => 300;

    [BindProperty]
    public TimeDetailsUserInput UserInput { get; set; } = new();

    public Time_Details_At_LocationModel(IRequestDistributedCache connectionRequestCache)
        : base(ServiceJourneyPage.Time_Details_At_Location, connectionRequestCache)
    {
    }

    protected override void OnGetWithError()
    {
        //todo: redo
        UserInput = ServiceModel!.UserInput!;
    }

    protected override void OnGetWithModel()
    {
        //todo: redo

        //if (ServiceModel!.HasTimeDetails == true)
        //{
        //    UserInput.HasDetails = true;
        //    UserInput.Description = ServiceModel.TimeDescription;
        //}
        //else if (ServiceModel!.HasTimeDetails == false)
        //{
        //    UserInput.HasDetails = false;
        //}
    }

    protected override IActionResult OnPostWithModel()
    {
        if (!UserInput.HasDetails.HasValue)
        {
            return RedirectToSelf(UserInput, ErrorId.Time_Details__MissingSelection);
        }

        if (UserInput.HasDetails == true && string.IsNullOrWhiteSpace(UserInput.Description))
        {
            return RedirectToSelf(UserInput, ErrorId.Time_Details__MissingText);
        }

        if (UserInput.HasDetails == true && !string.IsNullOrWhiteSpace(UserInput.Description) && UserInput.Description.Replace("\r", "").Length > MaxLength)
        {
            return RedirectToSelf(UserInput, ErrorId.Time_Details__DescriptionTooLong);
        }

        //string? newTimeDescription;
        //if (UserInput.HasDetails == true)
        //{
        //    ServiceModel!.HasTimeDetails = true;
        //    newTimeDescription = UserInput.Description;
        //}
        //else
        //{
        //    ServiceModel!.HasTimeDetails = false;
        //    newTimeDescription = null;
        //}

        //ServiceModel!.Updated = ServiceModel.Updated || ServiceModel.TimeDescription != newTimeDescription;
        //ServiceModel.TimeDescription = newTimeDescription;

        return NextPage();
    }
}