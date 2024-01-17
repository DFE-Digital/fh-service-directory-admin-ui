using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.SharedKernel.Razor.Time;

namespace FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;

//todo: rename to TimesViewModel?
public record TimesViewModels(
    TimeViewModel WeekdaysStarts,
    TimeViewModel WeekdaysFinishes,
    TimeViewModel WeekendsStarts,
    TimeViewModel WeekendsFinishes)
{
    private static TimeComponent WeekdaysStartsComponent => new("weekdaysStarts", "Starts", "weekdays-times-hint");
    private static TimeComponent WeekdaysFinishesComponent => new("weekdaysFinishes", "Finishes", "weekdays-times-hint", AmPm.Pm);
    private static TimeComponent WeekendsStartsComponent => new("weekendsStarts", "Starts", "weekends-times-hint");
    private static TimeComponent WeekendsFinishesComponent => new("weekendsFinishes", "Finishes", "weekends-times-hint", AmPm.Pm);

    public TimesViewModels(
        TimeModel? weekdaysStart,
        TimeModel? weekdaysFinish,
        TimeModel? weekendsStarts,
        TimeModel? weekendsFinish)
        : this(
            new TimeViewModel(WeekdaysStartsComponent, weekdaysStart),
            new TimeViewModel(WeekdaysFinishesComponent, weekdaysFinish),
            new TimeViewModel(WeekendsStartsComponent, weekendsStarts),
            new TimeViewModel(WeekendsFinishesComponent, weekendsFinish))
    {
    }

    //todo: need to support nullable?
    public TimesViewModels(TimesModels? timesModels)
        : this(timesModels?.WeekdaysStarts,
            timesModels?.WeekdaysFinishes,
            timesModels?.WeekendsStarts,
            timesModels?.WeekendsFinishes)
    {
    }

    public TimesViewModels(
        DateTime? weekdaysStart,
        DateTime? weekdaysFinish,
        DateTime? weekendsStarts,
        DateTime? weekendsFinish)
        : this(
            new TimeViewModel(WeekdaysStartsComponent, weekdaysStart),
            new TimeViewModel(WeekdaysFinishesComponent, weekdaysFinish),
            new TimeViewModel(WeekendsStartsComponent, weekendsStarts),
            new TimeViewModel(WeekendsFinishesComponent, weekendsFinish))
    {
    }

    public static TimesModels GetTimesFromForm(IFormCollection form)
    {
        return new TimesModels(
            WeekdaysStartsComponent.CreateModel(form),
            WeekdaysFinishesComponent.CreateModel(form),
            WeekendsStartsComponent.CreateModel(form),
            WeekendsFinishesComponent.CreateModel(form));
    }
}