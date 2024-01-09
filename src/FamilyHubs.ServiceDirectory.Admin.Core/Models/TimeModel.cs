using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

//todo: belongs in components
//todo: don't want to store static component in cache

public enum AmPm
{
    Am,
    Pm
}

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
        return new TimeModel(Name, form);
    }
}

//todo: TimeViewModel and TimeModel?

//todo: record?
public class TimeViewModel
{
    public TimeComponent Component { get; }
    public TimeModel? Time { get; }

    public TimeViewModel(TimeComponent component, DateTime? time = null)
    {
        Component = component;
        Time = time != null ? new TimeModel(time) : null;
    }

    public TimeViewModel(TimeComponent component, TimeModel? time)
    {
        Component = component;
        Time = time;
    }
}

[DebuggerDisplay("{ToDateTime() ?? \"<Empty>\"}")]
public class TimeModel
{
    public int? Hour { get; set; }
    public int? Minute { get; set; }
    public AmPm? AmPm { get; set; }

    private static readonly TimeZoneInfo UkTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");

    //todo: support null, or just have a null TimeModel?
    public TimeModel(DateTime? time = null)
    {
        if (time == null)
        {
            return;
        }

        if (time.Value.Hour > 12)
        {
            Hour = time.Value.Hour - 12;
            AmPm = Models.AmPm.Pm;
        }
        else
        {
            Hour = time.Value.Hour;
            AmPm = Models.AmPm.Am;
        }
        Minute = time.Value.Minute;
    }

    public TimeModel(string name, IFormCollection form)
    {
        if (int.TryParse(form[$"{name}Hour"].ToString(), out var value))
        {
            Hour = value;
        }
        if (int.TryParse(form[$"{name}Minute"].ToString(), out value))
        {
            Minute = value;
        }
        AmPm = form[$"{name}AmPm"].ToString() switch
        {
            "am" => Models.AmPm.Am,
            "pm" => Models.AmPm.Pm,
            _ => null
        };
    }

    public bool IsEmpty => Hour == null && Minute == null && AmPm == null;

    public bool IsHourValid => Hour is >= 0 and <= 12;
    public bool IsMinuteValid => Minute is >= 0 and <= 59;
    public bool IsValid => IsHourValid && IsMinuteValid && AmPm != null;

    public DateTime? ToDateTime()
    {
        if (Hour == null || Minute == null || AmPm == null)
        {
            return null;
        }

        var hour = AmPm == Models.AmPm.Pm ? Hour + 12 : Hour;

        //todo: unit test : utc to uk timezone correct?
        //        return TimeZoneInfo.ConvertTime(new DateTime(1, 1, 1, hour.Value, Minute.Value, 0, DateTimeKind.Utc), UkTimeZone);
        return TimeZoneInfo.ConvertTime(default(DateTime).AddHours(hour.Value).AddMinutes(Minute.Value), UkTimeZone);
    }
}