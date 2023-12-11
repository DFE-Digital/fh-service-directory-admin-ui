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
            .Add(ErrorId.What_Language__EnterLanguages, "Enter any languages the service is offered in")
        ;
}