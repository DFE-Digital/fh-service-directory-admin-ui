using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

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
                if (weekday != null)
                {
                    WeekdaysStarts = new TimeViewModel(WeekdaysStartsComponent, weekday.OpensAt);
                    WeekdaysFinishes = new TimeViewModel(WeekdaysFinishesComponent, weekday.ClosesAt);
                }
                var weekend = service.RegularSchedules
                    .FirstOrDefault(s => s is { Freq: FrequencyType.Weekly, ByDay: "SA,SU" });
                if (weekend != null)
                {
                    WeekendsStarts = new TimeViewModel(WeekendsStartsComponent, weekend.OpensAt);
                    WeekendsFinishes = new TimeViewModel(WeekendsFinishesComponent, weekend.ClosesAt);
                }
                break;
            case JourneyFlow.Add:
                WeekdaysStarts = new TimeViewModel(WeekdaysStartsComponent, ServiceModel!.WeekdaysStarts);
                WeekdaysFinishes = new TimeViewModel(WeekdaysFinishesComponent, ServiceModel.WeekdaysFinishes);
                WeekendsStarts = new TimeViewModel(WeekendsStartsComponent, ServiceModel.WeekendsStarts);
                WeekendsFinishes = new TimeViewModel(WeekendsFinishesComponent, ServiceModel.WeekendsFinishes);
                break;
        }
    }

    protected override Task<IActionResult> OnPostWithModelAsync(CancellationToken cancellationToken)
    {
        var weekdaysStarts = WeekdaysStartsComponent.CreateModel(Request.Form);
        var weekdaysFinishes = WeekdaysStartsComponent.CreateModel(Request.Form);
        var weekendsStarts = WeekdaysStartsComponent.CreateModel(Request.Form);
        var weekendsFinishes = WeekdaysStartsComponent.CreateModel(Request.Form);

        switch (Flow)
        {
            case JourneyFlow.Edit:
                break;
            case JourneyFlow.Add:
                ServiceModel!.WeekdaysStarts = weekdaysStarts;
                ServiceModel.WeekdaysFinishes = weekdaysFinishes;
                ServiceModel.WeekendsStarts = weekendsStarts;
                ServiceModel.WeekendsFinishes = weekendsFinishes;
                break;
        }

        return Task.FromResult(NextPage());
    }
}