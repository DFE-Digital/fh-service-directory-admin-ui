//using fh_service_directory_api.core.Interfaces.Entities;
//using LocalAuthorityInformationServices.SharedKernel;

//namespace fh_service_directory_api.core.Entities;

//public class Service : EntityBase<string>, IService
//{
//    private Service() { }

//    public Service
//    (
//        string id,
//        string name,
//        string? description,
//        string? accreditations,
//        DateTime? assured_date,
//        string? attending_access,
//        string? attending_type,
//        string? deliverable_type,
//        string? status,
//        string? url,
//        string? email,
//        string? fees,
//        ICollection<IEligibility>? eligibilitys,
//        ICollection<IFunding>? fundings,
//        ICollection<IHolidaySchedule>? holiday_schedules,
//        ICollection<ILanguage>? languages,
//        ICollection<IRegularSchedule>? regular_schedules,
//        ICollection<IReview>? reviews,
//        ICollection<IContact>? contacts,
//        ICollection<ICostOption>? cost_options,
//        ICollection<IServiceArea>? service_areas,
//        ICollection<IServiceAtLocation>? service_at_locations,
//        ICollection<IServiceTaxonomy>? service_taxonomys
//    )
//    {
//        Id = id;
//        Name = name;
//        Description = description;
//        Accreditations = accreditations;
//        Assured_date = assured_date;
//        Attending_access = attending_access;
//        Attending_type = attending_type;
//        Deliverable_type = deliverable_type;
//        Status = status;
//        Url = url;
//        Email = email;
//        Fees = fees;
//        Eligibilitys = eligibilitys;
//        Fundings = fundings;
//        Holiday_schedules = holiday_schedules;
//        Languages = languages;
//        Regular_schedules = regular_schedules;
//        Reviews = reviews;
//        Contacts = contacts;
//        Cost_options = cost_options;
//        Service_areas = service_areas;
//        Service_at_locations = service_at_locations;
//        Service_taxonomys = service_taxonomys;
//    }

//    public string Name { get; init; } = default!;

//    public string? Description { get; init; }

//    public string? Accreditations { get; init; }

//    public DateTime? Assured_date { get; init; }

//    public string? Attending_access { get; init; }

//    public string? Attending_type { get; init; }

//    public string? Deliverable_type { get; init; }

//    public string? Status { get; init; }

//    public string? Url { get; init; }

//    public string? Email { get; init; }

//    public string? Fees { get; init; }

//    public ICollection<IEligibility>? Eligibilitys { get; init; }

//    public ICollection<IFunding>? Fundings { get; init; }

//    public ICollection<IHolidaySchedule>? Holiday_schedules { get; init; }

//    public ICollection<ILanguage>? Languages { get; init; }

//    public ICollection<IRegularSchedule>? Regular_schedules { get; init; }

//    public ICollection<IReview>? Reviews { get; init; }

//    public ICollection<IContact>? Contacts { get; init; }

//    public ICollection<ICostOption>? Cost_options { get; init; }

//    public ICollection<IServiceArea>? Service_areas { get; init; }

//    public ICollection<IServiceAtLocation>? Service_at_locations { get; init; }

//    public ICollection<IServiceTaxonomy>? Service_taxonomys { get; init; }
//}
