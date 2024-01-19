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

            //todo: pass to TimesViewModels ctor? then could make view model immutable
            TimesViewModels.WeekdaysStarts.Error = Errors.GetErrorIfTriggered(
                (int)ErrorId.Times__EnterWeekdaysStartTime,
                (int)ErrorId.Times__EnterValidWeekdaysStartTime);

            TimesViewModels.WeekdaysFinishes.Error = Errors.GetErrorIfTriggered(
                (int)ErrorId.Times__EnterWeekdaysFinishTime,
                (int)ErrorId.Times__EnterValidWeekdaysFinishTime);

            TimesViewModels.WeekendsStarts.Error = Errors.GetErrorIfTriggered(
                (int)ErrorId.Times__EnterWeekendsStartTime,
                (int)ErrorId.Times__EnterValidWeekendsStartTime);

            TimesViewModels.WeekendsFinishes.Error = Errors.GetErrorIfTriggered(
                (int)ErrorId.Times__EnterWeekendsFinishTime,
                (int)ErrorId.Times__EnterValidWeekendsFinishTime);

            return;
        }

        switch (Flow)
        {
            case JourneyFlow.Edit:
                //todo: if edit flow, get service in base
                var service = await _serviceDirectoryClient.GetServiceById(ServiceId!.Value, cancellationToken);

                var weekday = service.Schedules
                    .FirstOrDefault(s => s is { Freq: FrequencyType.Weekly, ByDay: ByDayWeekdays });

                var weekend = service.Schedules
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

        bool weekdays = DayTypes.Contains(DayType.Weekdays);
        bool weekends = DayTypes.Contains(DayType.Weekends);

        var timesModels = TimesViewModels.GetTimesFromForm(weekdays, weekends, Request.Form);

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
        if (!timesModels.Weekdays)
        {
            timesModels.WeekdaysStarts = TimeModel.Empty;
            timesModels.WeekdaysFinishes = TimeModel.Empty;
        }

        if (!timesModels.Weekends)
        {
            timesModels.WeekendsStarts = TimeModel.Empty;
            timesModels.WeekendsFinishes = TimeModel.Empty;
        }
    }

    private List<ErrorId> GetTimeErrors(TimesModels timesModels)
    {
        List<ErrorId> errors = new();

        if (timesModels.Weekdays)
        {
            if (timesModels.WeekdaysStarts.IsEmpty)
            {
                errors.Add(ErrorId.Times__EnterWeekdaysStartTime);
            }
            else if (!timesModels.WeekdaysStarts.IsValid)
            {
                errors.Add(ErrorId.Times__EnterValidWeekdaysStartTime);
            }

            if (timesModels.WeekdaysFinishes.IsEmpty)
            {
                errors.Add(ErrorId.Times__EnterWeekdaysFinishTime);
            }
            else if (!timesModels.WeekdaysFinishes.IsValid)
            {
                errors.Add(ErrorId.Times__EnterValidWeekdaysFinishTime);
            }
        }

        if (timesModels.Weekends)
        {
            if (timesModels.WeekendsStarts.IsEmpty)
            {
                errors.Add(ErrorId.Times__EnterWeekendsStartTime);
            }
            else if (!timesModels.WeekendsStarts.IsValid)
            {
                errors.Add(ErrorId.Times__EnterValidWeekendsStartTime);
            }

            if (timesModels.WeekendsFinishes.IsEmpty)
            {
                errors.Add(ErrorId.Times__EnterWeekendsFinishTime);
            }
            else if (!timesModels.WeekendsFinishes.IsValid)
            {
                errors.Add(ErrorId.Times__EnterValidWeekendsFinishTime);
            }
        }

        return errors;
    }

    private async Task UpdateWhen(TimesModels times, CancellationToken cancellationToken)
    {
        var service = await _serviceDirectoryClient.GetServiceById(ServiceId!.Value, cancellationToken);

        service.Schedules = new List<ScheduleDto>();

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

        service.Schedules.Add(new ScheduleDto
        {
            Freq = FrequencyType.Weekly,
            ByDay = days == DayType.Weekdays ? ByDayWeekdays : ByDayWeekends,
            OpensAt = startTime,
            ClosesAt = finishesTime
        });
    }
}