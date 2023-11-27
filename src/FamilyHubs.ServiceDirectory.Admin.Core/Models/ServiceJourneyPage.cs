
namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

//todo: use these to construct error ids?
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

    public static string GetStartPagePath()
    {
        return $"/{(ServiceJourneyPage.Initiator+1).GetPageUrl()}";
    }

    public static string GetInitiatorPagePath(JourneyFlow flow)
    {
        switch (flow)
        {
            case JourneyFlow.Add:
            case JourneyFlow.AddRedo:
                return "/Welcome";
            case JourneyFlow.Edit:
                return $"/{ServiceJourneyPage.Details.GetPageUrl()}";
            default:
                throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
        }
    }
}