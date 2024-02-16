
using System.Runtime.CompilerServices;

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
    public static string GetPagePath(this LocationJourneyPage page, JourneyFlow flow, Journey journey)
    {
        if (page == LocationJourneyPage.Initiator)
        {
            switch (journey)
            {
                case Journey.Location:
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
                    break;
                case Journey.Service:
                    //todo: do we need to carry around the service journey flow?
                    return ServiceJourneyPage.Add_Location.GetPagePath(JourneyFlow.Add);
                default:
                    throw new SwitchExpressionException(journey);
            }
        }

        return $"/manage-locations/{page.GetPageUrl()}";
    }

    public static LocationJourneyPage GetAddFlowStartPage()
    {
        return LocationJourneyPage.Initiator + 1;
    }

    public static string GetAddFlowStartPagePath(Journey journey)
    {
        //todo: add flow and journey in GetPathPath (obvs renamed)?
        return $"{GetAddFlowStartPage().GetPagePath(JourneyFlow.Add, journey)}?flow={JourneyFlow.Add}&journey={journey}";
    }

    public static string GetEditStartPagePath()
    {
        return $"/manage-locations/{LocationJourneyPage.Location_Details.GetPageUrl()}?flow={JourneyFlow.Edit}";
    }
}