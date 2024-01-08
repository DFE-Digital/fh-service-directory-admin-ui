using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public enum AmPm
{
    Am,
    Pm
}

//todo: separate for name/desc & values?
//todo: doesn't belong with page. have in components?
public class TimeComponent
{
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

    public string? Description { get; set; }
    //todo: not nice having element id out of view
    public string? HintId { get; set; }
    public string Name { get; set; }
    public AmPm DefaultAmPm { get; set; }

    //public int Hour { get; set; }
    //public int Minute { get; set; }
    //public AmPm AmPm { get; set; }
}

public class TimeModel
{
    public int? Hour { get; set; }
    public int? Minute { get; set; }
    public AmPm? AmPm { get; set; }

    public TimeModel(string name, IFormCollection form)
    {
        int value;
        if (int.TryParse(form[$"{name}Hour"].ToString(), out value))
        {
            Hour = value;
        }
        if (int.TryParse(form[$"{name}Minute"].ToString(), out value))
        {
            Minute = value;
        }
        AmPm = form[$"{name}AmPm"].ToString() switch
        {
            "am" => manage_services.AmPm.Am,
            "pm" => manage_services.AmPm.Pm,
            _ => null
        };
    }
}

public class timesModel : ServicePageModel
{
    public static TimeComponent WeekdaysStarts => new("weekdaysStarts", "Starts", "weekdays-times-hint");
    public static TimeComponent WeekdaysFinishes => new("weekdaysFinishes", "Finishes", "weekdays-times-hint", AmPm.Pm);
    public static TimeComponent WeekendsStarts => new("weekendsStarts", "Starts", "weekends-times-hint");
    public static TimeComponent WeekendsFinishes => new("weekendsFinishes", "Finishes", "weekends-times-hint", AmPm.Pm);

    public timesModel(IRequestDistributedCache connectionRequestCache)
        : base(ServiceJourneyPage.Times, connectionRequestCache)
    {
    }

    protected override Task OnGetWithModelAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected override Task<IActionResult> OnPostWithModelAsync(CancellationToken cancellationToken)
    {
        var weekdaysStarts = new TimeModel(WeekdaysStarts.Name, Request.Form);
        var weekdaysFinishes = new TimeModel(WeekdaysFinishes.Name, Request.Form);
        var weekendsStarts = new TimeModel(WeekendsStarts.Name, Request.Form);
        var weekendsFinishes = new TimeModel(WeekendsFinishes.Name, Request.Form);

        return Task.FromResult(NextPage());
    }
}