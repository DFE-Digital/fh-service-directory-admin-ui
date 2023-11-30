
namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

//todo: dictionary errorid to invaliduserinput, save having to Zip the two arrays together
public record ServiceErrorState<TInput>(
    ServiceJourneyPage Page,
    ErrorId[] Errors,
    TInput? UserInput) where TInput : class;
