using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class TimeDetailsUserInput
{
    public bool? HasDetails { get; set; }
    public string? Description { get; set; }
}

public class Time_DetailsModel : ServicePageModel<TimeDetailsUserInput>
{
    public string TextBoxLabel { get; set; } = "Can you provide more details about when people can use this service?";
    public int? MaxLength => 300;

    [BindProperty]
    public TimeDetailsUserInput UserInput { get; set; } = new();

    public Time_DetailsModel(IRequestDistributedCache connectionRequestCache)
        : base(ServiceJourneyPage.Time_Details, connectionRequestCache)
    {
    }

    protected override void OnGetWithModel()
    {
        if (Errors.HasErrors)
        {
            UserInput = ServiceModel!.UserInput!;
            return;
        }

        if (ServiceModel!.HasTimeDetails.HasValue && ServiceModel!.HasTimeDetails!.Value)
        {
            UserInput.HasDetails = true;
            UserInput.Description = ServiceModel!.TimeDescription!;
        }
        else if (ServiceModel!.HasTimeDetails.HasValue && !ServiceModel!.HasTimeDetails.Value)
        {
            UserInput.HasDetails = false;
        }
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

        if (UserInput.HasDetails == true)
        {
            ServiceModel!.HasTimeDetails = true;
            ServiceModel!.TimeDescription = UserInput.Description;
        }
        else
        {
            ServiceModel!.HasTimeDetails = false;
            ServiceModel!.TimeDescription = null;
        }

        return NextPage();
    }
}