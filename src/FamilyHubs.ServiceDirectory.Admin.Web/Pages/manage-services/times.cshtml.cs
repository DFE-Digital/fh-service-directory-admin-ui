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

    public TimeModel WeekdaysStarts { get; set; }
    public TimeModel WeekdaysFinishes { get; set; }
    public TimeModel WeekendsStarts { get; set; }
    public TimeModel WeekendsFinishes { get; set; }

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
                    WeekdaysStarts = new TimeModel(WeekdaysStartsComponent, weekday.OpensAt);
                    WeekdaysFinishes = new TimeModel(WeekdaysFinishesComponent, weekday.ClosesAt);
                }
                var weekend = service.RegularSchedules
                    .FirstOrDefault(s => s is { Freq: FrequencyType.Weekly, ByDay: "SA,SU" });
                if (weekend != null)
                {
                    WeekendsStarts = new TimeModel(WeekendsStartsComponent, weekend.OpensAt);
                    WeekendsFinishes = new TimeModel(WeekendsFinishesComponent, weekend.ClosesAt);
                }
                break;
            case JourneyFlow.Add:
                break;
        }
    }

    protected override Task<IActionResult> OnPostWithModelAsync(CancellationToken cancellationToken)
    {
        WeekdaysStarts = WeekdaysStartsComponent.CreateModel(Request.Form);
        WeekdaysFinishes = WeekdaysStartsComponent.CreateModel(Request.Form);
        WeekendsStarts = WeekdaysStartsComponent.CreateModel(Request.Form);
        WeekendsFinishes = WeekdaysStartsComponent.CreateModel(Request.Form);

        switch (Flow)
        {
            case JourneyFlow.Edit:
                break;
            case JourneyFlow.Add:
                ServiceModel!.WeekdaysStarts = WeekdaysStarts;
                ServiceModel.WeekdaysFinishes = WeekdaysFinishes;
                ServiceModel.WeekendsStarts = WeekendsStarts;
                ServiceModel.WeekendsFinishes = WeekendsFinishes;
                break;
        }

        return Task.FromResult(NextPage());
    }
}