
namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

//todo: 1 generic version

public record ServiceErrorState(
    ServiceJourneyPage Page,
    ErrorId[] Errors);

public record LocationErrorState(
    LocationJourneyPage Page,
    ErrorId[] Errors);
