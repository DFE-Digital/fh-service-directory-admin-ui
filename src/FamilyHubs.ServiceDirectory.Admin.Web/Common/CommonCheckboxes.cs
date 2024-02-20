using FamilyHubs.ServiceDirectory.Shared.ReferenceData.ICalendar;
using FamilyHubs.SharedKernel.Razor.FullPages.Checkboxes;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Common;

//todo: move to components?
public static class CommonCheckboxes
{
    public static Checkbox[] DaysOfTheWeek => new[]
    {
        new Checkbox("Monday", DayCode.MO.ToString()),
        new Checkbox("Tuesday", DayCode.TU.ToString()),
        new Checkbox("Wednesday", DayCode.WE.ToString()),
        new Checkbox("Thursday", DayCode.TH.ToString()),
        new Checkbox("Friday", DayCode.FR.ToString()),
        new Checkbox("Saturday", DayCode.SA.ToString()),
        new Checkbox("Sunday", DayCode.SU.ToString())
    };
}