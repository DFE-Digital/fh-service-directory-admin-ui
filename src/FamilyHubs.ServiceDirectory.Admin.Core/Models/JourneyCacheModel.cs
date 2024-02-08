using System.Diagnostics;
using System.Text.Json;

namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

//todo: move into web components?
public class JourneyCacheModel<TJourneyPage, TErrorId, TUserInput>
    where TJourneyPage : struct, Enum, IConvertible
    where TErrorId : struct, Enum, IConvertible
    where TUserInput : class?
{
    public ErrorState<TJourneyPage, TErrorId>? ErrorState { get; set; }

    public void AddErrorState(TJourneyPage page, TErrorId[] errors)
    {
        ErrorState = new ErrorState<TJourneyPage, TErrorId>(page, errors);
    }
    
    public string? UserInputType { get; set; }
    public string? UserInputJson { get; set; }
    public TUserInput? UserInput { get; set; }

    public void PopulateUserInput()
    {
        Debug.Assert(UserInput == null);

        if (UserInputType != null && UserInputJson != null
            && UserInputType == typeof(TUserInput).FullName)
        {
            UserInput = JsonSerializer.Deserialize<TUserInput>(UserInputJson);
            UserInputJson = null;
        }
    }

    public void SetUserInput(TUserInput userInput)
    {
        if (userInput == null)
        {
            UserInputType = null;
            UserInputJson = null;
            return;
        }
        UserInputType = typeof(TUserInput).FullName;
        UserInputJson = JsonSerializer.Serialize(userInput);
    }

    public bool Updated { get; set; }
}