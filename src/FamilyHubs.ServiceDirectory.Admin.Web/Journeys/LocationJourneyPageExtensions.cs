using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Models.LocationJourney;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Journeys;

//todo: lots common with ServiceJourneyPageExtensions

// some methods are not extension methods, but they seem to belong here. maybe move them somewhere else (or rename the class)
//todo: we _could_ make this a generic class, passing in the url prefix etc. especially when JourneyFlow is generic
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
                    //todo: consumers are going to add query params to welcome, which aren't needed
                    //todo: back is welcome/manage depending on the user
                    // can we move initiator handling our of here, as we don;t want to always have to pass in the user
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

    public static string GetAddFlowStartPagePath(Journey journey, JourneyFlow? parentJourneyFlow)
    {
        //todo: add flow and journey in GetPathPath (obvs renamed)?

        string parentJourneyFlowParam = parentJourneyFlow == null ? "" : $"&parentJourneyFlow={parentJourneyFlow}";

        return $"{GetAddFlowStartPage().GetPagePath(JourneyFlow.Add)}?flow={JourneyFlow.Add}&journey={journey}{parentJourneyFlowParam}";
    }

    public static string GetEditStartPagePath()
    {
        return $"/manage-locations/{LocationJourneyPage.Location_Details.GetPageUrl()}?flow={JourneyFlow.Edit}";
    }
}