
namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

//todo: move into web components?
public class JourneyCacheModel<TJourneyPage, TErrorId, TUserInput>
    where TJourneyPage : struct, Enum, IConvertible
    where TErrorId : struct, Enum, IConvertible
{
    public ErrorState<TJourneyPage, TErrorId>? ErrorState { get; set; }

    public void AddErrorState(TJourneyPage page, TErrorId[] errors)
    {
        ErrorState = new ErrorState<TJourneyPage, TErrorId>(page, errors);
    }
    
    public string? UserInputType { get; set; }
    public TUserInput? UserInput { get; set; }
}