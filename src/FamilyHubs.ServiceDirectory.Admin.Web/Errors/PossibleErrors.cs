using System.Collections.Immutable;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.SharedKernel.Razor.ErrorNext;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Errors;

public static class PossibleErrors
{
    public static readonly ImmutableDictionary<int, PossibleError> All =
        ImmutableDictionary.Create<int, PossibleError>()
            .Add(ErrorId.Service_Name__EnterNameOfService, "Enter the name of the service")
            .Add(ErrorId.Support_Offered__SelectCategory, "Select the type of support the service offers")
            .Add(ErrorId.Support_Offered__SelectSubCategory, "Select name of sub-category support")
            .Add(ErrorId.Who_For__SelectChildrensService, "Select yes if the support offered by this service is related to children or young people")
            .Add(ErrorId.Who_For__SelectFromAge, "Select age from")
            .Add(ErrorId.Who_For__SelectToAge, "Select age to")
            .Add(ErrorId.Who_For__FromAgeAfterToAge, "The selected age to is lower than the age from")
            .Add(ErrorId.Who_For__SameAges, "Ages from and to cannot be the same")
            .Add(ErrorId.Service_Description__EnterDescriptionOfService, "Enter a description of the service")
            .Add(ErrorId.Service_Description__TooLong, "Service description must be 200 characters or less")
            .Add(ErrorId.What_Language__EnterLanguages, "Enter any languages the service is offered in")
            .Add(ErrorId.What_Language__SelectLanguageOnce, "You can only select a language once")
            .Add(ErrorId.What_Language__EnterSupportedLanguage, "Enter an available language")
            .Add(ErrorId.Service_Cost__MissingSelection, "Select whether it costs money to use this service")
            .Add(ErrorId.Service_Cost__DescriptionTooLong, "Cost description must be 150 characters or less")
            .Add(ErrorId.Times__SelectWhenServiceAvailable, "Select when the service is available")
            .Add(ErrorId.Times__EnterWeekdaysTimes, "Enter times for weekdays")
            .Add(ErrorId.Times__EnterWeekendsTimes, "Enter times for weekends")
            .Add(ErrorId.Times__EnterValidWeekdaysStartTime, "Enter a valid start time for weekdays")
            .Add(ErrorId.Times__EnterValidWeekdaysFinishTime, "Enter a valid finish time for weekdays")
            .Add(ErrorId.Times__EnterValidWeekendsStartTime, "Enter a valid start time for weekends")
            .Add(ErrorId.Times__EnterValidWeekendsFinishTime, "Enter a valid finish time for weekends")
        ;
}