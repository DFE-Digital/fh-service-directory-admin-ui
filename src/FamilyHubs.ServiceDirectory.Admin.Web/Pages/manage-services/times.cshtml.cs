using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.SharedKernel.Razor.Time;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public enum DayType
{
    Weekdays,
    Weekends
}

//todo: if time empty, have error message per empty field, rather than one error message for 2 empty fields
// will need 2 separate error ids for the link from the summary to work, but they'll be the same message
// have max 1 error message per time field, either valid or empty

//todo: when the user returns to this page using the back button,
// the browser (Edge at least) issues a GET request, but ignores the form data returned
// and populates the form with the original data instead.
// this is apparently by design, but it's not what we want.
// it means that if they e.g. select weekends, enter a time, then deselect weekends,
// then continue, then go back, the weekends time is still there even though we explicitly clear it

public class timesModel : ServicePageModel<TimesModels>
{
    //todo: belong in components?
    private const string ByDayWeekdays = "MO,TU,WE,TH,FR";
    private const string ByDayWeekends = "SA,SU";
        
    [BindProperty]
    public List<DayType> DayTypes { get; set; }

    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public TimesViewModels? TimesViewModels { get; set; }

    public timesModel(
        IRequestDistributedCache connectionRequestCache,
        IServiceDirectoryClient serviceDirectoryClient)
        : base(ServiceJourneyPage.Times, connectionRequestCache)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
        DayTypes = new List<DayType>();
    }

    protected override async Task OnGetWithModelAsync(CancellationToken cancellationToken)
    {
        if (Errors.HasErrors)
        {
            //todo: could have array of components and models and zip them
            TimesViewModels = new TimesViewModels(ServiceModel!.UserInput);
            
            TimesViewModels.WeekdaysStarts.Error = Errors.GetErrorIfTriggered(
                (int)ErrorId.Times__EnterWeekdaysTimes,
                (int)ErrorId.Times__EnterValidWeekdaysStartTime);
            return;
        }

        switch (Flow)
        {
            case JourneyFlow.Edit:
                //todo: if edit flow, get service in base
                var service = await _serviceDirectoryClient.GetServiceById(ServiceId!.Value, cancellationToken);

                var weekday = service.RegularSchedules
                    .FirstOrDefault(s => s is { Freq: FrequencyType.Weekly, ByDay: ByDayWeekdays });

                var weekend = service.RegularSchedules
                    .FirstOrDefault(s => s is { Freq: FrequencyType.Weekly, ByDay: ByDayWeekends });

                TimesViewModels = new TimesViewModels(
                    weekday?.OpensAt, weekday?.ClosesAt,
                    weekend?.OpensAt, weekend?.ClosesAt);
                break;

            default:
                TimesViewModels = new TimesViewModels(ServiceModel!.Times);
                break;
        }
    }

    protected override async Task<IActionResult> OnPostWithModelAsync(CancellationToken cancellationToken)
    {
        if (!DayTypes.Any())
        {
            return RedirectToSelf(ErrorId.Times__SelectWhenServiceAvailable);
        }

        var timesModels = TimesViewModels.GetTimesFromForm(Request.Form);

        var errors = GetTimeErrors(timesModels);
        if (errors.Any())
        {
            return RedirectToSelf(timesModels, errors.ToArray());
        }

        ClearTimesIfDayTypeNotSelected(timesModels);

        switch (Flow)
        {
            case JourneyFlow.Edit:
                await UpdateWhen(timesModels, cancellationToken);
                break;
            case JourneyFlow.Add:
                ServiceModel!.Times = timesModels;
                break;
        }

        return NextPage();
    }

    private void ClearTimesIfDayTypeNotSelected(TimesModels timesModels)
    {
        // if checkbox not ticked, don't save any entered values
        if (!DayTypes.Contains(DayType.Weekdays))
        {
            timesModels.WeekdaysStarts = TimeModel.Empty;
            timesModels.WeekdaysFinishes = TimeModel.Empty;
        }

        if (!DayTypes.Contains(DayType.Weekends))
        {
            timesModels.WeekendsStarts = TimeModel.Empty;
            timesModels.WeekendsFinishes = TimeModel.Empty;
        }
    }

    private List<ErrorId> GetTimeErrors(TimesModels timesModels)
    {
        List<ErrorId> errors = new();

        if (DayTypes.Contains(DayType.Weekdays))
        {
            if (timesModels.WeekdaysStarts.IsEmpty || timesModels.WeekdaysFinishes.IsEmpty)
            {
                errors.Add(ErrorId.Times__EnterWeekdaysTimes);
            }

            if (timesModels.WeekdaysStarts is { IsEmpty: false, IsValid: false })
            {
                errors.Add(ErrorId.Times__EnterValidWeekdaysStartTime);
            }

            if (timesModels.WeekdaysFinishes is { IsEmpty: false, IsValid: false })
            {
                errors.Add(ErrorId.Times__EnterValidWeekdaysFinishTime);
            }
        }

        if (DayTypes.Contains(DayType.Weekends))
        {
            if (timesModels.WeekendsStarts.IsEmpty || timesModels.WeekendsFinishes.IsEmpty)
            {
                errors.Add(ErrorId.Times__EnterWeekendsTimes);
            }
            
            if (timesModels.WeekendsStarts is { IsEmpty: false, IsValid: false })
            {
                errors.Add(ErrorId.Times__EnterValidWeekendsStartTime);
            }

            if (timesModels.WeekendsFinishes is { IsEmpty: false, IsValid: false })
            {
                errors.Add(ErrorId.Times__EnterValidWeekendsFinishTime);
            }
        }

        return errors;
    }

    private async Task UpdateWhen(TimesModels times, CancellationToken cancellationToken)
    {
        var service = await _serviceDirectoryClient.GetServiceById(ServiceId!.Value, cancellationToken);

        service.RegularSchedules = new List<RegularScheduleDto>();

        AddToSchedule(service, DayType.Weekdays, times.WeekdaysStarts, times.WeekdaysFinishes);
        AddToSchedule(service, DayType.Weekends, times.WeekendsStarts, times.WeekendsFinishes);

        await _serviceDirectoryClient.UpdateService(service, cancellationToken);
    }

    private static void AddToSchedule(ServiceDto service, DayType days, TimeModel starts, TimeModel finishes)
    {
        var startTime = starts.ToDateTime();
        var finishesTime = finishes.ToDateTime();
        if (startTime == null || finishesTime == null)
        {
            return;
        }

        //todo: throw if one but not the other?

        service.RegularSchedules.Add(new RegularScheduleDto
        {
            Freq = FrequencyType.Weekly,
            ByDay = days == DayType.Weekdays ? ByDayWeekdays : ByDayWeekends,
            OpensAt = startTime,
            ClosesAt = finishesTime
        });
    }
}