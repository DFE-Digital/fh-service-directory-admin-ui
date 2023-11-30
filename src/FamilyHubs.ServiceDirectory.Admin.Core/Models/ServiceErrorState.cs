
namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

public record ServiceErrorState(
    ServiceJourneyPage Page,
    ErrorId[] Errors);