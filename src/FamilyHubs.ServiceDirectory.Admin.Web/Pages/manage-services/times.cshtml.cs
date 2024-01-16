using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

//todo: there should be only one
public enum DayType
{
    Weekdays,
    Weekends
}

public enum Days
{
    Weekdays,
    Weekends
}

public record TimesViewModels(
    TimeViewModel WeekdaysStarts,
    TimeViewModel WeekdaysFinishes,
    TimeViewModel WeekendsStarts,
    TimeViewModel WeekendsFinishes)
{
    private static TimeComponent WeekdaysStartsComponent => new("weekdaysStarts", "Starts", "weekdays-times-hint");
    private static TimeComponent WeekdaysFinishesComponent => new("weekdaysFinishes", "Finishes", "weekdays-times-hint", AmPm.Pm);
    private static TimeComponent WeekendsStartsComponent => new("weekendsStarts", "Starts", "weekends-times-hint");
    private static TimeComponent WeekendsFinishesComponent => new("weekendsFinishes", "Finishes", "weekends-times-hint", AmPm.Pm);

    public TimesViewModels(
        TimeModel? weekdaysStart,
        TimeModel? weekdaysFinish,
        TimeModel? weekendsStarts,
        TimeModel? weekendsFinish)
        : this(
        new TimeViewModel(WeekdaysStartsComponent, weekdaysStart),
        new TimeViewModel(WeekdaysFinishesComponent, weekdaysFinish),
        new TimeViewModel(WeekendsStartsComponent, weekendsStarts),
        new TimeViewModel(WeekendsFinishesComponent, weekendsFinish))
    {
    }

    //todo: need to support nullable?
    public TimesViewModels(TimesModels? timesModels)
        : this(timesModels?.WeekdaysStarts,
            timesModels?.WeekdaysFinishes,
            timesModels?.WeekendsStarts,
            timesModels?.WeekendsFinishes)
    {
    }

    public TimesViewModels(
        DateTime? weekdaysStart,
        DateTime? weekdaysFinish,
        DateTime? weekendsStarts,
        DateTime? weekendsFinish)
        : this(
            new TimeViewModel(WeekdaysStartsComponent, weekdaysStart),
            new TimeViewModel(WeekdaysFinishesComponent, weekdaysFinish),
            new TimeViewModel(WeekendsStartsComponent, weekendsStarts),
            new TimeViewModel(WeekendsFinishesComponent, weekendsFinish))
    {
    }
}

//public class TimesViewModels
//{
//    public TimeViewModel WeekdaysStarts { get; }
//    public TimeViewModel WeekdaysFinishes { get; }
//    public TimeViewModel WeekendsStarts { get; }
//    public TimeViewModel WeekendsFinishes { get; }

//    public TimesViewModels(
//        TimeViewModel weekdaysStarts,
//        TimeViewModel weekdaysFinishes,
//        TimeViewModel weekendsStarts,
//        TimeViewModel weekendsFinishes)
//    {
//        WeekdaysStarts = weekdaysStarts;
//        WeekdaysFinishes = weekdaysFinishes;
//        WeekendsStarts = weekendsStarts;
//        WeekendsFinishes = weekendsFinishes;
//    }
//}

public class timesModel : ServicePageModel<TimesModels>
{
    [BindProperty]
    public List<DayType> DayType { get; set; }

    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public TimesViewModels? TimesViewModels { get; set; }

    public timesModel(
        IRequestDistributedCache connectionRequestCache,
        IServiceDirectoryClient serviceDirectoryClient)
        : base(ServiceJourneyPage.Times, connectionRequestCache)
    {
        //todo: nullability TimeModel
        _serviceDirectoryClient = serviceDirectoryClient;
        DayType = new List<DayType>();
    }

    protected override async Task OnGetWithModelAsync(CancellationToken cancellationToken)
    {
        //todo: javascript disabled
        if (Errors.HasErrors)
        {
            //todo: could have array of components and models and zip them
            if (ServiceModel!.UserInput != null)
            {
                TimesViewModels = new TimesViewModels(ServiceModel!.UserInput);
            }
            return;
        }

        switch (Flow)
        {
            case JourneyFlow.Edit:
                //todo: if edit flow, get service in base
                var service = await _serviceDirectoryClient.GetServiceById(ServiceId!.Value, cancellationToken);

                var weekday = service.RegularSchedules
                    .FirstOrDefault(s => s is { Freq: FrequencyType.Weekly, ByDay: "MO,TU,WE,TH,FR" });

                var weekend = service.RegularSchedules
                    .FirstOrDefault(s => s is { Freq: FrequencyType.Weekly, ByDay: "SA,SU" });

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
        if (!DayType.Any())
        {
            return RedirectToSelf(ErrorId.Times__SelectWhenServiceAvailable);
        }

        TimesModels timesModels = new()
        {
            WeekdaysStarts = WeekdaysStartsComponent.CreateModel(Request.Form),
            WeekdaysFinishes = WeekdaysFinishesComponent.CreateModel(Request.Form),
            WeekendsStarts = WeekendsStartsComponent.CreateModel(Request.Form),
            WeekendsFinishes = WeekendsFinishesComponent.CreateModel(Request.Form)
        };

        var errors = GetTimeErrors(timesModels);
        if (errors.Any())
        {
            return RedirectToSelf(timesModels, errors.ToArray());
        }

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

    private List<ErrorId> GetTimeErrors(TimesModels timesModels)
    {
        List<ErrorId> errors = new();

        if (DayType.Contains(manage_services.DayType.Weekdays))
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

        if (DayType.Contains(manage_services.DayType.Weekends))
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

        AddToSchedule(service, Days.Weekdays, times.WeekdaysStarts, times.WeekdaysFinishes);
        AddToSchedule(service, Days.Weekends, times.WeekendsStarts, times.WeekendsFinishes);

        await _serviceDirectoryClient.UpdateService(service, cancellationToken);
    }

    private static void AddToSchedule(ServiceDto service, Days days, TimeModel starts, TimeModel finishes)
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
            //todo: no magic strings
            ByDay = days == Days.Weekdays ? "MO,TU,WE,TH,FR" : "SA,SU",
            OpensAt = startTime,
            ClosesAt = finishesTime
        });
    }
}