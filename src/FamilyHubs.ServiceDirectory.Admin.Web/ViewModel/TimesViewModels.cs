using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.SharedKernel.Razor.Time;

namespace FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;

public record TimesViewModels(
    bool Weekdays,
    bool Weekends,
    TimeViewModel WeekdaysStarts,
    TimeViewModel WeekdaysFinishes,
    TimeViewModel WeekendsStarts,
    TimeViewModel WeekendsFinishes)
{
    private static TimeComponent WeekdaysStartsComponent => new("weekdaysStarts", "Starts", "weekdays-times-hint");
    private static TimeComponent WeekdaysFinishesComponent => new("weekdaysFinishes", "Finishes", "weekdays-times-hint", AmPm.Pm);
    private static TimeComponent WeekendsStartsComponent => new("weekendsStarts", "Starts", "weekends-times-hint");
    private static TimeComponent WeekendsFinishesComponent => new("weekendsFinishes", "Finishes", "weekends-times-hint", AmPm.Pm);

    public TimesViewModels(TimesModels? timesModels)
        : this(timesModels?.Weekdays ?? false, timesModels?.Weekends ?? false,
            new TimeViewModel(WeekdaysStartsComponent, timesModels?.WeekdaysStarts),
            new TimeViewModel(WeekdaysFinishesComponent, timesModels?.WeekdaysFinishes),
            new TimeViewModel(WeekendsStartsComponent, timesModels?.WeekendsStarts),
            new TimeViewModel(WeekendsFinishesComponent, timesModels?.WeekendsFinishes))
    {
    }

    public TimesViewModels(
        DateTime? weekdaysStart,
        DateTime? weekdaysFinish,
        DateTime? weekendsStarts,
        DateTime? weekendsFinish)
        : this(
            weekdaysStart != null || weekdaysFinish != null,
            weekendsStarts != null || weekendsFinish != null,
            new TimeViewModel(WeekdaysStartsComponent, weekdaysStart),
            new TimeViewModel(WeekdaysFinishesComponent, weekdaysFinish),
            new TimeViewModel(WeekendsStartsComponent, weekendsStarts),
            new TimeViewModel(WeekendsFinishesComponent, weekendsFinish))
    {
    }

    public static TimesModels GetTimesFromForm(bool weekdays, bool weekends, IFormCollection form)
    {
        return new TimesModels(
            weekdays, weekends,
            WeekdaysStartsComponent.CreateModel(form),
            WeekdaysFinishesComponent.CreateModel(form),
            WeekendsStartsComponent.CreateModel(form),
            WeekendsFinishesComponent.CreateModel(form));
    }
}