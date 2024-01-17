using FamilyHubs.SharedKernel.Razor.Time;

namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

public class TimesModels
{
    public TimesModels(
        TimeModel weekdaysStarts, TimeModel weekdaysFinishes,
        TimeModel weekendsStarts, TimeModel weekendsFinishes)
    {
        WeekdaysStarts = weekdaysStarts;
        WeekdaysFinishes = weekdaysFinishes;
        WeekendsStarts = weekendsStarts;
        WeekendsFinishes = weekendsFinishes;
    }

    //todo: array and enum index?
    public TimeModel WeekdaysStarts { get; set; }
    public TimeModel WeekdaysFinishes { get; set; }
    public TimeModel WeekendsStarts { get; set; }
    public TimeModel WeekendsFinishes { get; set; }
}