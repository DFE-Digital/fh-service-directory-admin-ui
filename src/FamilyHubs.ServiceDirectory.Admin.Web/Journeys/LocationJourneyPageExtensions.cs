using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Models.LocationJourney;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Journeys;

//todo: lots common with ServiceJourneyPageExtensions

// some methods are not extension methods, but they seem to belong here. maybe move them somewhere else (or rename the class)
//todo: we _could_ make this a generic class, passing in the url prefix etc. especially when JourneyFlow is generic
public static class LocationJourneyPageExtensions
{
    //todo: remove 'Page' from method names?
    private static string GetSlug(this LocationJourneyPage page)
    {
        return page.ToString().Replace('_', '-');
    }

    public static string GetPagePath(this LocationJourneyPage page, JourneyFlow flow)
    {
        return $"/manage-locations/{page.GetSlug()}?flow={flow.ToUrlString()}";
    }

    private static LocationJourneyPage GetAddFlowStartPage()
    {
        return LocationJourneyPage.Initiator + 1;
    }

    public static string GetAddFlowStartPagePath(Journey journey, string? parentJourneyContext)
    {
        //todo: add journey in GetPathPath (obvs renamed)?

        string parentJourneyContextParam = parentJourneyContext == null ? "" : $"&parentJourneyContext={parentJourneyContext}";

        return $"{GetAddFlowStartPage().GetPagePath(JourneyFlow.Add)}&journey={journey}{parentJourneyContextParam}";
    }

    public static string GetEditStartPagePath()
    {
        return LocationJourneyPage.Location_Details.GetPagePath(JourneyFlow.Edit);
    }
}