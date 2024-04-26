using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Models.ServiceJourney;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Journeys;

public static class ServiceJourneyPageExtensions
{
    public static ServiceJourneyPage FromSlug(string slugifiedServiceJourneyPage)
    {
        return (ServiceJourneyPage)Enum.Parse(typeof(ServiceJourneyPage), slugifiedServiceJourneyPage.Replace('-', '_'), true);
    }

    public static string GetSlug(this ServiceJourneyPage page)
    {
        return page.ToString().Replace('_', '-');
    }

    public static string GetPagePath(string slugifiedServiceJourneyPage)
    {
        return $"/manage-services/{slugifiedServiceJourneyPage}";
    }

    //todo: flow is only needed for initiator page. move initiator logic to where called for initiator page and remove flow param?
    public static string GetPagePath(this ServiceJourneyPage page, JourneyFlow flow)
    {
        if (page == ServiceJourneyPage.Initiator)
        {
            throw new InvalidOperationException(
                "Can't get path of initiator page here. (It should be handled elsewhere.)");

            //switch (flow)
            //{
            //    case JourneyFlow.Add:
            //        //todo: consumers are going to add query params to welcome, which aren't needed
            //        //todo: back is only welcome if user is a dfe admin, otherwise it's the services page
            //        // can we move initiator handling our of here, as we don;t want to always have to pass in the user
            //        return "/Welcome";
            //    case JourneyFlow.Edit:
            //        // details is both the initiator and the final page of the journey
            //        page = ServiceJourneyPage.Service_Detail;
            //        break;
            //    default:
            //        throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
            //}
        }

        return $"/manage-services/{page.GetSlug()}";
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
        return $"/manage-services/{ServiceJourneyPage.Service_Detail.GetSlug()}?flow={JourneyFlow.Edit}";
    }
}