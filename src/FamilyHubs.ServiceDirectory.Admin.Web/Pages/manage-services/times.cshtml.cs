using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public enum Days
{
    Weekdays,
    Weekends
}

public class timesModel : ServicePageModel
{
    private readonly IServiceDirectoryClient _serviceDirectoryClient;
    public static TimeComponent WeekdaysStartsComponent => new("weekdaysStarts", "Starts", "weekdays-times-hint");
    public static TimeComponent WeekdaysFinishesComponent => new("weekdaysFinishes", "Finishes", "weekdays-times-hint", AmPm.Pm);
    public static TimeComponent WeekendsStartsComponent => new("weekendsStarts", "Starts", "weekends-times-hint");
    public static TimeComponent WeekendsFinishesComponent => new("weekendsFinishes", "Finishes", "weekends-times-hint", AmPm.Pm);

    public TimeViewModel WeekdaysStarts { get; set; }
    public TimeViewModel WeekdaysFinishes { get; set; }
    public TimeViewModel WeekendsStarts { get; set; }
    public TimeViewModel WeekendsFinishes { get; set; }

    public timesModel(
        IRequestDistributedCache connectionRequestCache,
        IServiceDirectoryClient serviceDirectoryClient)
        : base(ServiceJourneyPage.Times, connectionRequestCache)
    {
        //todo: nullability TimeModel
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    protected override async Task OnGetWithModelAsync(CancellationToken cancellationToken)
    {
        switch (Flow)
        {
            case JourneyFlow.Edit:
                //todo: if edit flow, get service in base
                var service = await _serviceDirectoryClient.GetServiceById(ServiceId!.Value, cancellationToken);

                var weekday = service.RegularSchedules
                    .FirstOrDefault(s => s is { Freq: FrequencyType.Weekly, ByDay: "MO,TU,WE,TH,FR" });

                WeekdaysStarts = new TimeViewModel(WeekdaysStartsComponent, weekday?.OpensAt);
                WeekdaysFinishes = new TimeViewModel(WeekdaysFinishesComponent, weekday?.ClosesAt);

                var weekend = service.RegularSchedules
                    .FirstOrDefault(s => s is { Freq: FrequencyType.Weekly, ByDay: "SA,SU" });

                WeekendsStarts = new TimeViewModel(WeekendsStartsComponent, weekend?.OpensAt);
                WeekendsFinishes = new TimeViewModel(WeekendsFinishesComponent, weekend?.ClosesAt);
                break;
            case JourneyFlow.Add:
                WeekdaysStarts = new TimeViewModel(WeekdaysStartsComponent, ServiceModel!.WeekdaysStarts);
                WeekdaysFinishes = new TimeViewModel(WeekdaysFinishesComponent, ServiceModel.WeekdaysFinishes);
                WeekendsStarts = new TimeViewModel(WeekendsStartsComponent, ServiceModel.WeekendsStarts);
                WeekendsFinishes = new TimeViewModel(WeekendsFinishesComponent, ServiceModel.WeekendsFinishes);
                break;
        }
    }

    protected override async Task<IActionResult> OnPostWithModelAsync(CancellationToken cancellationToken)
    {
        var weekdaysStarts = WeekdaysStartsComponent.CreateModel(Request.Form);
        var weekdaysFinishes = WeekdaysFinishesComponent.CreateModel(Request.Form);
        var weekendsStarts = WeekendsStartsComponent.CreateModel(Request.Form);
        var weekendsFinishes = WeekendsFinishesComponent.CreateModel(Request.Form);

        List<ErrorId> errors = new();

        //todo: need to handle combination of missing and invalid
        if (!weekdaysStarts.IsValid)
        {
            errors.Add(ErrorId.Times__EnterValidWeekdaysStartTime);
        }
        if (!weekdaysFinishes.IsValid)
        {
            errors.Add(ErrorId.Times__EnterValidWeekdaysFinishTime);
        }
        if (!weekendsStarts.IsValid)
        {
            errors.Add(ErrorId.Times__EnterValidWeekendsStartTime);
        }
        if (!weekendsFinishes.IsValid)
        {
            errors.Add(ErrorId.Times__EnterValidWeekendsFinishTime);
        }
        if (weekdaysStarts.IsEmpty || weekdaysFinishes.IsEmpty)
        {
            errors.Add(ErrorId.Times__EnterWeekdaysTimes);
        }
        if (weekendsStarts.IsEmpty || weekendsFinishes.IsEmpty)
        {
            errors.Add(ErrorId.Times__EnterWeekendsTimes);
        }

        if (errors.Any())
        {
            return RedirectToSelf(errors);
        }

        switch (Flow)
        {
            case JourneyFlow.Edit:
                await UpdateWhen(weekdaysStarts, weekdaysFinishes, weekendsStarts, weekendsFinishes, cancellationToken);
                break;
            case JourneyFlow.Add:
                ServiceModel!.WeekdaysStarts = weekdaysStarts;
                ServiceModel.WeekdaysFinishes = weekdaysFinishes;
                ServiceModel.WeekendsStarts = weekendsStarts;
                ServiceModel.WeekendsFinishes = weekendsFinishes;
                break;
        }

        return NextPage();
    }

    private async Task UpdateWhen(
        TimeModel weekdaysStarts,
        TimeModel weekdaysFinishes,
        TimeModel weekendsStarts,
        TimeModel weekendsFinishes,
        CancellationToken cancellationToken)
    {
        var service = await _serviceDirectoryClient.GetServiceById(ServiceId!.Value, cancellationToken);

        service.RegularSchedules = new List<RegularScheduleDto>();

        AddToSchedule(service, Days.Weekdays, weekdaysStarts, weekdaysFinishes);
        AddToSchedule(service, Days.Weekends, weekendsStarts, weekendsFinishes);

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