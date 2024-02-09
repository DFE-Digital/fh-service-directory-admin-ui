
namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

//todo: use these to construct error ids?
//todo: instead of using enum directly and extension methods, have wrapper class?
public enum ServiceJourneyPage
{
    /// <summary>
    /// Represents the page(s) where the journey can be initiated.
    /// </summary>
    Initiator,

    Local_Authority,
    Service_Name,
    Support_Offered,
    Service_Description,
    Who_For,
    What_Language,
    Service_Cost,
    Times,
    Service_More_Details,
    /// <summary>
    /// The service details page.
    /// </summary>
    Service_Detail
}

//todo: move to web, or keep together?
public static class ServiceJourneyPageExtensions
{
    //todo: remove 'Page' from method names?
    private static string GetPageUrl(this ServiceJourneyPage page)
    {
        return page.ToString().Replace('_', '-');
    }

    //todo: flow is only needed for initiator page. move initiator logic to where called for initiator page and remove flow param?
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
                    page = ServiceJourneyPage.Service_Detail;
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

    public static string GetEditStartPagePath()
    {
        return $"/manage-services/{ServiceJourneyPage.Service_Detail.GetPageUrl()}?flow={JourneyFlow.Edit}";
    }
}