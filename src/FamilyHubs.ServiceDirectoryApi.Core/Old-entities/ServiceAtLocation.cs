//using fh_service_directory_api.core.Interfaces.Entities;
//using LocalAuthorityInformationServices.SharedKernel;

//namespace fh_service_directory_api.core.Entities;

//public class ServiceAtLocation : EntityBase<string>, IServiceAtLocation
//{
//    private ServiceAtLocation() { }
//    public ServiceAtLocation
//    (
//        string id,
//        ILocation location,
//        ICollection<IHolidaySchedule>? holidayScheduleCollection,
//        ICollection<IRegularSchedule>? regular_schedule
//    )
//    {
//        Id = id;
//        Location = location;
//        HolidayScheduleCollection = holidayScheduleCollection;
//        Regular_schedule = regular_schedule;
//    }

//    public ILocation Location { get; init; } = default!;

//    public virtual ICollection<IHolidaySchedule>? HolidayScheduleCollection { get; init; }

//    public virtual ICollection<IRegularSchedule>? Regular_schedule { get; init; }
//}
