
namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

//todo: use these to construct error ids?
//todo: instead of using enum directly and extension methods, have ServiceJourneyPage be a class an internal enum?
public enum ServiceJourneyPage
{
    /// <summary>
    /// Represents the page(s) where the journey can be initiated.
    /// </summary>
    Initiator,

    Service_Name,
    Support_Offered,

    /// <summary>
    /// The service details page.
    /// </summary>
    Details
}

//todo: move to web, or keep together?
public static class ServiceJourneyPageExtensions
{
    //todo: have both GetPageUrl and GetPageName? or a combined one?
    public static string GetPageUrl(this ServiceJourneyPage page)
    {
        return page.ToString().Replace('_', '-');
    }

    public static ServiceJourneyPage GetAddFlowStartPage()
    {
        return ServiceJourneyPage.Initiator + 1;
    }

    public static string GetAddFlowStartPagePath()
    {
        return $"/{GetAddFlowStartPage().GetPageUrl()}?flow={JourneyFlow.Add}";
    }

    public static string GetInitiatorPagePath(JourneyFlow? flow)
    {
        switch (flow)
        {
            case JourneyFlow.Add:
            case JourneyFlow.AddRedo:
            case null:
                return "/Welcome";
            case JourneyFlow.Edit:
                return $"/{ServiceJourneyPage.Details.GetPageUrl()}";
            default:
                throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
        }
    }
}