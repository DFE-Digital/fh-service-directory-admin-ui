using FamilyHubs.SharedKernel.Razor.Time;

namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

public class TimesModels
{
    public TimesModels(
        bool weekdays, bool weekends,
        TimeModel weekdaysStarts, TimeModel weekdaysFinishes,
        TimeModel weekendsStarts, TimeModel weekendsFinishes)
    {
        Weekdays = weekdays;
        Weekends = weekends;
        WeekdaysStarts = weekdaysStarts;
        WeekdaysFinishes = weekdaysFinishes;
        WeekendsStarts = weekendsStarts;
        WeekendsFinishes = weekendsFinishes;
    }

    public bool Weekdays { get; }
    public bool Weekends { get; }

    //todo: array and enum index?
    public TimeModel WeekdaysStarts { get; set; }
    public TimeModel WeekdaysFinishes { get; set; }
    public TimeModel WeekendsStarts { get; set; }
    public TimeModel WeekendsFinishes { get; set; }
}