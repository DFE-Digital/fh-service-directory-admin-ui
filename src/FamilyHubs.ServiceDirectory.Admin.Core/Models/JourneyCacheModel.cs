
namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

//todo: move into web components?
public class JourneyCacheModel<TJourneyPage, TErrorId, TUserInput>
    where TJourneyPage : struct, Enum, IConvertible
    where TErrorId : struct, Enum, IConvertible
{
    public ErrorState<TJourneyPage, TErrorId>? ErrorState { get; set; }

    //todo: ErrorState factory method?

    public string? UserInputType { get; set; }
    public TUserInput? UserInput { get; set; }
}