using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using IdentityModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using System;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

//todo: weekdays: FREQ=WEEKLY;BYDAY=MO,TU,WE,TH,FR
//weekends: FREQ=WEEKLY;BYDAY=SA,SU
//both: 2 entries??

public enum AmPm
{
    Am,
    Pm
}

//todo: doesn't belong with page. have in components?
public class TimeComponent
{
    public string? Description { get; set; }
    //todo: not nice having element id out of view
    public string? HintId { get; set; }
    public string Name { get; set; }
    public AmPm DefaultAmPm { get; set; }

    public TimeComponent(
        string name,
        string? description = null,
        string? hintId = null,
        AmPm defaultAmPm = AmPm.Am)
    {
        Name = name;
        Description = description;
        HintId = hintId;
        DefaultAmPm = defaultAmPm;
    }

    public TimeModel CreateModel(IFormCollection form)
    {
        return new TimeModel(this, form);
    }
}

public class TimeModel
{
    public TimeComponent Component { get; set; }
    public int? Hour { get; set; }
    public int? Minute { get; set; }
    public AmPm? AmPm { get; set; }

    public TimeModel(TimeComponent component, DateTime? time)
    {
        Component = component;

        if (time == null)
        {
            return;
        }

        if (time.Value.Hour > 12)
        {
            Hour = time.Value.Hour - 12;
            AmPm = manage_services.AmPm.Pm;
        }
        else
        {
            Hour = time.Value.Hour;
            AmPm = manage_services.AmPm.Am;
        }
        Minute = time.Value.Minute;
    }

    public TimeModel(TimeComponent component, IFormCollection form)
    {
        Component = component;

        if (int.TryParse(form[$"{component.Name}Hour"].ToString(), out var value))
        {
            Hour = value;
        }
        if (int.TryParse(form[$"{component.Name}Minute"].ToString(), out value))
        {
            Minute = value;
        }
        AmPm = form[$"{component.Name}AmPm"].ToString() switch
        {
            "am" => manage_services.AmPm.Am,
            "pm" => manage_services.AmPm.Pm,
            _ => null
        };
    }
}

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

        return Task.FromResult(NextPage());
    }
}