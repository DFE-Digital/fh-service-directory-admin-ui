//using fh_service_directory_api.core.Interfaces.Entities;
//using LocalAuthorityInformationServices.SharedKernel;

//namespace fh_service_directory_api.core.Entities;

//public class HolidaySchedule : EntityBase<string>, IHolidaySchedule
//{
//    public bool Closed { get; init; }

//    public DateTime? Closes_at { get; init; }

//    public DateTime? Start_date { get; init; }

//    public DateTime? End_date { get; init; }

//    public DateTime? Opens_at { get; init; }

//    //public OpenReferralServiceAtLocation? Service_at_location { get; init; }

//    private HolidaySchedule() { }

//    public HolidaySchedule
//    (
//        bool closed,
//        DateTime? closes_at,
//        DateTime? start_date,
//        DateTime? end_date,
//        DateTime? opens_at
//    //OpenReferralServiceAtLocation? service_at_location
//    )
//    {
//        Closed = closed;
//        Closes_at = closes_at;
//        Start_date = start_date;
//        End_date = end_date;
//        Opens_at = opens_at;
//        //Service_at_location = service_at_location;
//    }
//}
