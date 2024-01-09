using Microsoft.AspNetCore.Http;

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
            AmPm = Models.AmPm.Pm;
        }
        else
        {
            Hour = time.Value.Hour;
            AmPm = Models.AmPm.Am;
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
            "am" => Models.AmPm.Am,
            "pm" => Models.AmPm.Pm,
            _ => null
        };
    }
}