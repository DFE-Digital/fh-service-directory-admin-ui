using FamilyHubs.SharedKernel.Razor.Time;

namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

public class TimesModels
{
    //todo: ctor or nullables? < this (warnings)
    //todo: array and enum index?
    public TimeModel WeekdaysStarts { get; set; }
    public TimeModel WeekdaysFinishes { get; set; }
    public TimeModel WeekendsStarts { get; set; }
    public TimeModel WeekendsFinishes { get; set; }
}