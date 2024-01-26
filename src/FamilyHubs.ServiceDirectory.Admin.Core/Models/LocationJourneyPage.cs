
namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

public enum LocationJourneyPage
{
    /// <summary>
    /// Represents the page(s) where the journey can be initiated.
    /// </summary>
    Initiator,

    Location_Address,
    Location_Information,

    /// <summary>
    /// The location details page.
    /// </summary>
    Location_Detail
}

//todo: lots common with ServiceJourneyPageExtensions

public static class LocationJourneyPageExtensions
{
    //todo: remove 'Page' from method names?
    private static string GetPageUrl(this LocationJourneyPage page)
    {
        return page.ToString().Replace('_', '-');
    }

    //todo: flow is only needed for initiator page. move initiator logic to where called for initiator page and remove flow param?
    public static string GetPagePath(this LocationJourneyPage page, JourneyFlow flow)
    {
        if (page == LocationJourneyPage.Initiator)
        {
            switch (flow)
            {
                case JourneyFlow.Add:
                case JourneyFlow.AddRedo:
                    //todo: consumers are going to add query params to welcome, which aren't needed
                    return "/Welcome";
                case JourneyFlow.Edit:
                    // details is both the initiator and the final page of the journey
                    page = LocationJourneyPage.Location_Detail;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
            }
        }

        return $"/manage-locations/{page.GetPageUrl()}";
    }

    public static LocationJourneyPage GetAddFlowStartPage()
    {
        return LocationJourneyPage.Initiator + 1;
    }

    public static string GetAddFlowStartPagePath()
    {
        return $"{GetAddFlowStartPage().GetPagePath(JourneyFlow.Add)}?flow={JourneyFlow.Add}";
    }
}