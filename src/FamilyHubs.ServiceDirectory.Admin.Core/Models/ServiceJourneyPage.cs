
namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

//todo: use these to construct error ids?
//todo: instead of using enum directly and extension methods, have wrapper class?
//todo: need to add support for non-linear journeys
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
    How_Use,
    Add_Location,
    Select_Location,
    Times,
    Time_Details,
    Contact,
    Service_More_Details,
    /// <summary>
    /// The service details page.
    /// </summary>
    Service_Detail,

    // have these here for now, but they won't stay here
    Times_At_Location
}
