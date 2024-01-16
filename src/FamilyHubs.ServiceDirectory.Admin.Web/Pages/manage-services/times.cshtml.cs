using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

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

public class TimesModels
{
    //todo: array and enum index?
    public TimeModel WeekdaysStarts { get; set; }
    public TimeModel WeekdaysFinishes { get; set; }
    public TimeModel WeekendsStarts { get; set; }
    public TimeModel WeekendsFinishes { get; set; }
}

public class timesModel : ServicePageModel<TimesModels>
{
    [BindProperty]
    public List<DayType> DayType { get; set; }

    private readonly IServiceDirectoryClient _serviceDirectoryClient;
    public static TimeComponent WeekdaysStartsComponent => new("weekdaysStarts", "Starts", "weekdays-times-hint");
    public static TimeComponent WeekdaysFinishesComponent => new("weekdaysFinishes", "Finishes", "weekdays-times-hint", AmPm.Pm);
    public static TimeComponent WeekendsStartsComponent => new("weekendsStarts", "Starts", "weekends-times-hint");
    public static TimeComponent WeekendsFinishesComponent => new("weekendsFinishes", "Finishes", "weekends-times-hint", AmPm.Pm);

    public TimeViewModel? WeekdaysStarts { get; set; }
    public TimeViewModel? WeekdaysFinishes { get; set; }
    public TimeViewModel? WeekendsStarts { get; set; }
    public TimeViewModel? WeekendsFinishes { get; set; }

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
            WeekdaysStarts = new TimeViewModel(WeekdaysStartsComponent, ServiceModel!.UserInput?.WeekdaysStarts);
            WeekdaysFinishes = new TimeViewModel(WeekdaysFinishesComponent, ServiceModel.UserInput?.WeekdaysFinishes);
            WeekendsStarts = new TimeViewModel(WeekendsStartsComponent, ServiceModel.UserInput?.WeekendsStarts);
            WeekendsFinishes = new TimeViewModel(WeekendsFinishesComponent, ServiceModel.UserInput?.WeekendsFinishes);
            return;
        }

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

        List<ErrorId> errors = new();

        //todo: need to handle combination of missing and invalid
        if (timesModels.WeekdaysStarts.IsEmpty || timesModels.WeekdaysFinishes.IsEmpty)
        {
            errors.Add(ErrorId.Times__EnterWeekdaysTimes);
        }
        if (timesModels.WeekendsStarts.IsEmpty || timesModels.WeekendsFinishes.IsEmpty)
        {
            errors.Add(ErrorId.Times__EnterWeekendsTimes);
        }
        if (timesModels.WeekdaysStarts is { IsEmpty: false, IsValid: false })
        {
            errors.Add(ErrorId.Times__EnterValidWeekdaysStartTime);
        }
        if (timesModels.WeekdaysFinishes is { IsEmpty: false, IsValid: false })
        {
            errors.Add(ErrorId.Times__EnterValidWeekdaysFinishTime);
        }
        if (timesModels.WeekendsStarts is { IsEmpty: false, IsValid: false })
        {
            errors.Add(ErrorId.Times__EnterValidWeekendsStartTime);
        }
        if (timesModels.WeekendsFinishes is { IsEmpty: false, IsValid: false })
        {
            errors.Add(ErrorId.Times__EnterValidWeekendsFinishTime);
        }

        if (errors.Any())
        {
            return RedirectToSelf(timesModels, errors.ToArray());
        }

        switch (Flow)
        {
            case JourneyFlow.Edit:
                //todo: pass timesModels
                await UpdateWhen(timesModels.WeekdaysStarts, timesModels.WeekdaysFinishes, timesModels.WeekendsStarts, timesModels.WeekendsFinishes, cancellationToken);
                break;
            case JourneyFlow.Add:
                //todo: store timesmodels????
                ServiceModel!.WeekdaysStarts = timesModels.WeekdaysStarts;
                ServiceModel.WeekdaysFinishes = timesModels.WeekdaysFinishes;
                ServiceModel.WeekendsStarts = timesModels.WeekendsStarts;
                ServiceModel.WeekendsFinishes = timesModels.WeekendsFinishes;
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