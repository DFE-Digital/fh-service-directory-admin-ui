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

    public static string GetPagePath(this ServiceJourneyPage page, JourneyFlow flow)
    {
        if (page == ServiceJourneyPage.Initiator)
        {
            throw new InvalidOperationException(
                "Can't get path of initiator page here. (It should be handled elsewhere.)");
        }

        return $"/manage-services/{page.GetSlug()}?flow={flow.ToUrlString()}";
    }

    private static ServiceJourneyPage GetAddFlowStartPage()
    {
        return ServiceJourneyPage.Initiator + 1;
    }

    public static string GetAddFlowStartPagePath()
    {
        return $"{GetAddFlowStartPage().GetPagePath(JourneyFlow.Add)}";
    }

    public static string GetEditStartPagePath()
    {
        return $"/manage-services/{ServiceJourneyPage.Service_Detail.GetSlug()}?flow={JourneyFlow.Edit}";
    }
}