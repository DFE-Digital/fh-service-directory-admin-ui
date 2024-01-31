
namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

public enum LocationJourneyPage
{
    /// <summary>
    /// Represents the page(s) where the journey can be initiated.
    /// </summary>
    Initiator,

    Location_Address,
    Family_Hub,
    Location_Information,

    /// <summary>
    /// The location details page.
    /// </summary>
    Location_Details
}

//todo: lots common with ServiceJourneyPageExtensions

// some methods are not extension methods, but they seem to belong here. maybe move them somewhere else (or rename the class)
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
                    return "/manage-locations";
                case JourneyFlow.Edit:
                    // details is both the initiator and the final page of the journey
                    page = LocationJourneyPage.Location_Details;
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

    public static string GetRedoPagePath(this LocationJourneyPage page)
    {
        return $"/manage-locations/{page.GetPageUrl()}?flow={JourneyFlow.AddRedo}";
    }

    public static string GetEditStartPagePath(long locationId)
    {
        return $"/manage-locations/{LocationJourneyPage.Location_Details.GetPageUrl()}?locationId={locationId}&flow={JourneyFlow.Edit}";
    }
}