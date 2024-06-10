using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Models.ServiceJourney;
using FamilyHubs.ServiceDirectory.Admin.Web.Journeys;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class Time_Details_At_LocationModel : ServicePageModel<TimeDetailsUserInput>
{
    public string? Title { get; set; }
    public int? MaxLength => 300;

    [BindProperty]
    public TimeDetailsUserInput UserInput { get; set; } = new();

    public Time_Details_At_LocationModel(IRequestDistributedCache connectionRequestCache)
        : base(ServiceJourneyPage.Time_Details_At_Location, connectionRequestCache)
    {
    }

    protected override void OnGetWithError()
    {
        UserInput = ServiceModel!.UserInput!;
        var location = ServiceModel!.CurrentLocation!;
        SetTitle(location);
    }

    protected override void OnGetWithModel()
    {
        string redo = Request.Query["redo"].ToString();
        if (redo != "")
        {
            BackUrl = $"{ServiceJourneyPageExtensions.GetPagePath(redo)}?flow={Flow}";
        }

        var location = GetLocation();
        SetTitle(location);

        if (location.HasTimeDetails == true)
        {
            UserInput.HasDetails = true;
            UserInput.Description = location.TimeDescription;
        }
        else if (location.HasTimeDetails == false)
        {
            UserInput.HasDetails = false;
        }
    }

    private ServiceLocationModel GetLocation()
    {
        string locationIdString = Request.Query["locationId"].ToString();
        if (locationIdString != "")
        {
            // user has asked to redo a specific location
            long locationId = long.Parse(locationIdString);

            return ServiceModel!.GetLocation(locationId);
        }

        return ServiceModel!.CurrentLocation!;
    }

    private void SetTitle(ServiceLocationModel location)
    {
        Title = $"Can you provide more details about using this service at {location.DisplayName}?";
    }

    protected override IActionResult OnPostWithModel()
    {
        if (!UserInput.HasDetails.HasValue)
        {
            return RedirectToSelf(UserInput, ErrorId.Time_Details__MissingSelection);
        }

        if (UserInput.HasDetails == true && string.IsNullOrWhiteSpace(UserInput.Description))
        {
            return RedirectToSelf(UserInput, ErrorId.Time_Details_At_Location__MissingText);
        }

        if (UserInput.HasDetails == true && !string.IsNullOrWhiteSpace(UserInput.Description) && UserInput.Description.Replace("\r", "").Length > MaxLength)
        {
            return RedirectToSelf(UserInput, ErrorId.Time_Details_At_Location__DescriptionTooLong);
        }

        var location = GetLocation();

        ServiceModel!.Updated = ServiceModel!.Updated || HaveTimesDescriptionAtLocationBeenUpdated(location);

        if (UserInput.HasDetails == true)
        {
            location.HasTimeDetails = true;
            location.TimeDescription = UserInput.Description;
        }
        else
        {
            location.HasTimeDetails = false;
            location.TimeDescription = null;
        }

        string redo = Request.Query["redo"].ToString();
        if (redo != "")
        {
            return Redirect(GetServicePageUrl(ServiceJourneyPageExtensions.FromSlug(redo)));
        }

        return NextPage();
    }

    private bool HaveTimesDescriptionAtLocationBeenUpdated(ServiceLocationModel location)
    {
        return location.TimeDescription != UserInput.Description;
    }
}