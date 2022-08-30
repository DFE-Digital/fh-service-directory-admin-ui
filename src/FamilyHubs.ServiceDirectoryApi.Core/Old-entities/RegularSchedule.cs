//using fh_service_directory_api.core.Interfaces.Entities;
//using LocalAuthorityInformationServices.SharedKernel;

//namespace fh_service_directory_api.core.Entities;

//public class RegularSchedule : EntityBase<string>, IRegularSchedule
//{
//    private RegularSchedule() { }

//    public RegularSchedule
//    (
//        string id,
//        string description,
//        DateTime? opens_at,
//        DateTime? closes_at,
//        string? byday,
//        string? bymonthday,
//        string? dtstart,
//        string? freq,
//        string? interval,
//        DateTime? valid_from,
//        DateTime? valid_to
//    //, OpenReferralServiceAtLocation? service_at_location
//    )
//    {
//        Id = id;
//        Description = description;
//        Opens_at = opens_at;
//        Closes_at = closes_at;
//        Byday = byday;
//        Bymonthday = bymonthday;
//        Dtstart = dtstart;
//        Freq = freq;
//        Interval = interval;
//        Valid_from = valid_from;
//        Valid_to = valid_to;
//        //Service_at_location = service_at_location;
//    }
//    public string Description { get; init; } = default!;

//    public DateTime? Opens_at { get; init; }

//    public DateTime? Closes_at { get; init; }

//    public string? Byday { get; init; }

//    public string? Bymonthday { get; init; }

//    public string? Dtstart { get; init; }

//    public string? Freq { get; init; }

//    public string? Interval { get; init; }

//    public DateTime? Valid_from
//    {
//        get; init;
//    }
//    public DateTime? Valid_to { get; init; }
//    //public OpenReferralServiceAtLocation? Service_at_location { get; init; }
//}
