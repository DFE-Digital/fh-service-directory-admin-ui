using System.Collections.Immutable;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.SharedKernel.Razor.ErrorNext;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Errors;

public static class PossibleErrors
{
    public static readonly ImmutableDictionary<int, PossibleError> All =
        ImmutableDictionary.Create<int, PossibleError>()
            .Add(ErrorId.Service_Name__EnterNameOfService, "Enter the name of the service")
            .Add(ErrorId.Who_For__SelectChildrensService, "Select whether children or young people can use this service")
            .Add(ErrorId.Who_For__SelectFromAge, "Select age from")
            .Add(ErrorId.Who_For__SelectToAge, "Select age to")
            .Add(ErrorId.Who_For__SameAges, "Ages from and to cannot be the same")
        ;
}