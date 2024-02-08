using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using FamilyHubs.SharedKernel.Razor.Time;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public enum DayType
{
    Weekdays,
    Weekends
}

public class timesModel : ServicePageModel<TimesModels>
{
    [BindProperty]
    public List<DayType> DayTypes { get; set; }

    public TimesViewModels? TimesViewModels { get; set; }

    public timesModel(IRequestDistributedCache connectionRequestCache)
        : base(ServiceJourneyPage.Times, connectionRequestCache)
    {
        DayTypes = new List<DayType>();
    }

    protected override void OnGetWithModel()
    {
        if (Errors.HasErrors)
        {
            //todo: could have array of components and models and zip them
            TimesViewModels = new TimesViewModels(ServiceModel!.UserInput);

            //todo: pass to TimesViewModels ctor? then could make view model immutable
            TimesViewModels.WeekdaysStarts.Error = Errors.GetErrorIfTriggered(
                (int)ErrorId.Times__EnterWeekdaysStartTime,
                (int)ErrorId.Times__EnterValidWeekdaysStartTime);

            TimesViewModels.WeekdaysFinishes.Error = Errors.GetErrorIfTriggered(
                (int)ErrorId.Times__EnterWeekdaysFinishTime,
                (int)ErrorId.Times__EnterValidWeekdaysFinishTime);

            TimesViewModels.WeekendsStarts.Error = Errors.GetErrorIfTriggered(
                (int)ErrorId.Times__EnterWeekendsStartTime,
                (int)ErrorId.Times__EnterValidWeekendsStartTime);

            TimesViewModels.WeekendsFinishes.Error = Errors.GetErrorIfTriggered(
                (int)ErrorId.Times__EnterWeekendsFinishTime,
                (int)ErrorId.Times__EnterValidWeekendsFinishTime);

            return;
        }

        TimesViewModels = new TimesViewModels(ServiceModel!.Times);
    }

    protected override IActionResult OnPostWithModel()
    {
        if (!DayTypes.Any())
        {
            return RedirectToSelf(ErrorId.Times__SelectWhenServiceAvailable);
        }

        bool weekdays = DayTypes.Contains(DayType.Weekdays);
        bool weekends = DayTypes.Contains(DayType.Weekends);

        var timesModels = TimesViewModels.GetTimesFromForm(weekdays, weekends, Request.Form);

        var errors = GetTimeErrors(timesModels);
        if (errors.Any())
        {
            return RedirectToSelf(timesModels, errors.ToArray());
        }

        ClearTimesIfDayTypeNotSelected(timesModels);

        ServiceModel!.Times = timesModels;

        return NextPage();
    }

    private void ClearTimesIfDayTypeNotSelected(TimesModels timesModels)
    {
        // if checkbox not ticked, don't save any entered values
        if (!timesModels.Weekdays)
        {
            timesModels.WeekdaysStarts = TimeModel.Empty;
            timesModels.WeekdaysFinishes = TimeModel.Empty;
        }

        if (!timesModels.Weekends)
        {
            timesModels.WeekendsStarts = TimeModel.Empty;
            timesModels.WeekendsFinishes = TimeModel.Empty;
        }
    }

    private List<ErrorId> GetTimeErrors(TimesModels timesModels)
    {
        List<ErrorId> errors = new();

        if (timesModels.Weekdays)
        {
            if (timesModels.WeekdaysStarts.IsEmpty)
            {
                errors.Add(ErrorId.Times__EnterWeekdaysStartTime);
            }
            else if (!timesModels.WeekdaysStarts.IsValid)
            {
                errors.Add(ErrorId.Times__EnterValidWeekdaysStartTime);
            }

            if (timesModels.WeekdaysFinishes.IsEmpty)
            {
                errors.Add(ErrorId.Times__EnterWeekdaysFinishTime);
            }
            else if (!timesModels.WeekdaysFinishes.IsValid)
            {
                errors.Add(ErrorId.Times__EnterValidWeekdaysFinishTime);
            }
        }

        if (timesModels.Weekends)
        {
            if (timesModels.WeekendsStarts.IsEmpty)
            {
                errors.Add(ErrorId.Times__EnterWeekendsStartTime);
            }
            else if (!timesModels.WeekendsStarts.IsValid)
            {
                errors.Add(ErrorId.Times__EnterValidWeekendsStartTime);
            }

            if (timesModels.WeekendsFinishes.IsEmpty)
            {
                errors.Add(ErrorId.Times__EnterWeekendsFinishTime);
            }
            else if (!timesModels.WeekendsFinishes.IsValid)
            {
                errors.Add(ErrorId.Times__EnterValidWeekendsFinishTime);
            }
        }

        return errors;
    }
}