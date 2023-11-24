using System.Collections.Immutable;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.SharedKernel.Razor.Errors;
using FamilyHubs.SharedKernel.Razor.FullPages;
using ErrorDictionary = System.Collections.Immutable.ImmutableDictionary<int, FamilyHubs.SharedKernel.Razor.Errors.Error>;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Errors;

public static class PossibleErrors
{
    //todo: use the new error handling
    public static readonly ErrorDictionary All = ImmutableDictionary
            .Create<int, Error>()
            .Add(ErrorId.Service_Name__EnterNameOfService, ISingleTextboxPageModel.TextBoxId, "Enter the name of the service")
        ;
}