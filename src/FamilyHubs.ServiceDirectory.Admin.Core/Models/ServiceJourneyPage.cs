
namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

//todo: use these to construct error ids?
//todo: instead of using enum directly and extension methods, have wrapper class?
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
    //todo: remove 'Page' from method names?
    private static string GetPageUrl(this ServiceJourneyPage page)
    {
        return page.ToString().Replace('_', '-');
    }

    public static string GetPagePath(this ServiceJourneyPage page, JourneyFlow flow)
    {
        if (page == ServiceJourneyPage.Initiator)
        {
            switch (flow)
            {
                case JourneyFlow.Add:
                case JourneyFlow.AddRedo:
                    //todo: consumers are going to add query params to welcome, which aren't needed
                    return "/Welcome";
                case JourneyFlow.Edit:
                    // details is both the initiator and the final page of the journey
                    page = ServiceJourneyPage.Details;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
            }
        }

        return $"/manage-services/{page.GetPageUrl()}";
    }

    public static ServiceJourneyPage GetAddFlowStartPage()
    {
        return ServiceJourneyPage.Initiator + 1;
    }

    public static string GetAddFlowStartPagePath()
    {
        return $"{GetAddFlowStartPage().GetPagePath(JourneyFlow.Add)}?flow={JourneyFlow.Add}";
    }
}