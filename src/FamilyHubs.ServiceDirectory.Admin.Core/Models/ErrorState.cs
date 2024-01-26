
namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

public record ErrorState<TJourneyPage, TErrorId>(
    TJourneyPage Page,
    TErrorId[] Errors)
    where TJourneyPage : struct, Enum, IConvertible
    where TErrorId : struct, Enum, IConvertible;
