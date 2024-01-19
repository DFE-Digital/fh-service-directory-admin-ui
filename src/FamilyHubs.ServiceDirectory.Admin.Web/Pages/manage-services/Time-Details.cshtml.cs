using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
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

    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    [BindProperty]
    public TimeDetailsUserInput UserInput { get; set; } = new();

    public Time_DetailsModel(
        IRequestDistributedCache connectionRequestCache,
        IServiceDirectoryClient serviceDirectoryClient)
    : base(ServiceJourneyPage.Time_Details, connectionRequestCache)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    protected override async Task OnGetWithModelAsync(CancellationToken cancellationToken)
    {
        if (Errors.HasErrors)
        {
            UserInput = ServiceModel!.UserInput!;
            return;
        }

        switch (Flow)
        {
            case JourneyFlow.Edit:
                var service = await _serviceDirectoryClient.GetServiceById(ServiceId!.Value, cancellationToken);

                if (service.Schedules.Any(x => x.Description != null))
                {
                    UserInput.HasDetails = true;
                    UserInput.Description = service.Schedules.First(x => x.Description != null).Description;
                }
                else
                {
                    UserInput.HasDetails = false;
                }
                break;

            default:
                if (ServiceModel!.HasTimeDetails.HasValue && ServiceModel!.HasTimeDetails!.Value)
                {
                    UserInput.HasDetails = true;
                    UserInput.Description = ServiceModel!.TimeDescription!;
                }
                else if (ServiceModel!.HasTimeDetails.HasValue && !ServiceModel!.HasTimeDetails.Value)
                {
                    UserInput.HasDetails = false;
                }
                break;
        }
    }

    protected override async Task<IActionResult> OnPostWithModelAsync(CancellationToken cancellationToken)
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

        switch (Flow)
        {
            case JourneyFlow.Edit:
                await UpdateTimeDescription(UserInput.HasDetails.Value, UserInput.Description!, cancellationToken);
                break;
            default:
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
                break;
        }

        return NextPage();
    }

    private async Task UpdateTimeDescription(bool hasTimeDescription, string description, CancellationToken cancellationToken)
    {
        var service = await _serviceDirectoryClient.GetServiceById(ServiceId!.Value, cancellationToken);
        var schedule = service.Schedules.FirstOrDefault(x => x.Description != null);

        if (hasTimeDescription)
        {
            if (schedule == null)
            {
                service.Schedules.Add(new() { Description = description });
            }
            else
            {
                schedule.Description = description;
            }
        }
        else if (schedule != null)
        {
            service.Schedules.Remove(schedule);
        }

        await _serviceDirectoryClient.UpdateService(service, cancellationToken);
    }
}
