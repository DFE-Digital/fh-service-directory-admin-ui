namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

public enum ErrorId
{
    Service_Name__EnterNameOfService,
    Support_Offered__SelectCategory,
    Support_Offered__SelectSubCategory,
    Service_Description__EnterDescriptionOfService,
    Service_Description__TooLong,
    Who_For__SelectChildrensService,
    Who_For__SelectFromAge,
    Who_For__SelectToAge,
    Who_For__FromAgeAfterToAge,
    Who_For__SameAges,
    What_Language__EnterLanguages,
    What_Language__SelectLanguageOnce,
    What_Language__EnterSupportedLanguage,
    Service_Cost__MissingSelection,
    Service_Cost__DescriptionTooLong,
    Times__SelectWhenServiceAvailable,
    Times__EnterWeekdaysStartTime,
    Times__EnterWeekdaysFinishTime,
    Times__EnterWeekendsStartTime,
    Times__EnterWeekendsFinishTime,
    Times__EnterValidWeekdaysStartTime,
    Times__EnterValidWeekdaysFinishTime,
    Times__EnterValidWeekendsStartTime,
    Times__EnterValidWeekendsFinishTime,
    Time_Details__MissingSelection,
    Time_Details__MissingText,
    Time_Details__DescriptionTooLong,
    Location_Information__TooLong
}