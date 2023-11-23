using System.Collections.Immutable;
using FamilyHubs.SharedKernel.Razor.Errors;
using FamilyHubs.SharedKernel.Razor.FullPages;
using ErrorDictionary = System.Collections.Immutable.ImmutableDictionary<int, FamilyHubs.SharedKernel.Razor.Errors.Error>;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Errors;

public enum ErrorId
{
    ServiceName_EnterNameOfService
}

public static class PossibleErrors
{
    //todo: use a tag helper for the error summary, and pass the htmlelementid to the tag helper (so that details of the view don't leak)
    public static readonly ErrorDictionary All = ImmutableDictionary
            .Create<int, Error>()
            .Add(ErrorId.ServiceName_EnterNameOfService, ISingleTextboxPageModel.TextBoxId, "Enter the name of the service")
        ;
}