using FamilyHubs.SharedKernel;
using FamilyHubs.SharedKernel.Interfaces;
using fh_service_directory_api.core.Interfaces.Entities;
using System.Collections.ObjectModel;

namespace fh_service_directory_api.core.Entities;

public class OpenReferralService : EntityBase<string>, IOpenReferralService, IAggregateRoot
{
    private OpenReferralService() { }

    public OpenReferralService(string id, string parentId, string name, string? description, string? accreditations, DateTime? assured_date, string? attending_access, string? attending_type, string? deliverable_type, string? status, string? url, string? email, string? fees
        , ICollection<OpenReferralServiceDelivery> serviceDelivery
        , ICollection<OpenReferralEligibility> eligibilitys
        , ICollection<OpenReferralFunding> fundings
        , ICollection<OpenReferralHoliday_Schedule> holiday_schedules
        , ICollection<OpenReferralLanguage> languages
        , ICollection<OpenReferralRegular_Schedule> regular_schedules
        , ICollection<OpenReferralReview> reviews
        , ICollection<OpenReferralContact> contacts
        , ICollection<OpenReferralCost_Option> cost_options
        , ICollection<OpenReferralService_Area> service_areas
        , ICollection<OpenReferralServiceAtLocation> service_at_locations
        , ICollection<OpenReferralService_Taxonomy> service_taxonomys
        )
    {
        Id = id;
        OpenReferralOrganisationId = parentId;
        Name = name;
        Description = description;
        Accreditations = accreditations;
        Assured_date = assured_date;
        Attending_access = attending_access;
        Attending_type = attending_type;
        Deliverable_type = deliverable_type;
        Status = status;
        Url = url;
        Email = email;
        Fees = fees;
        Eligibilitys = eligibilitys as ICollection<OpenReferralEligibility> ?? new Collection<OpenReferralEligibility>();
        Fundings = fundings as ICollection<OpenReferralFunding> ?? new Collection<OpenReferralFunding>();
        Holiday_schedules = holiday_schedules as ICollection<OpenReferralHoliday_Schedule> ?? new Collection<OpenReferralHoliday_Schedule>();
        Languages = languages as ICollection<OpenReferralLanguage> ?? new Collection<OpenReferralLanguage>();
        Regular_schedules = regular_schedules as ICollection<OpenReferralRegular_Schedule> ?? new Collection<OpenReferralRegular_Schedule>();
        Reviews = reviews as ICollection<OpenReferralReview> ?? new Collection<OpenReferralReview>();
        Contacts = contacts as ICollection<OpenReferralContact> ?? new Collection<OpenReferralContact>();
        Cost_options = cost_options as ICollection<OpenReferralCost_Option> ?? new Collection<OpenReferralCost_Option>();
        Service_areas = service_areas as ICollection<OpenReferralService_Area> ?? new Collection<OpenReferralService_Area>();
        Service_at_locations = service_at_locations as ICollection<OpenReferralServiceAtLocation> ?? new Collection<OpenReferralServiceAtLocation>();
        Service_taxonomys = service_taxonomys as ICollection<OpenReferralService_Taxonomy> ?? new Collection<OpenReferralService_Taxonomy>();
        ServiceDelivery = serviceDelivery as ICollection<OpenReferralServiceDelivery> ?? new Collection<OpenReferralServiceDelivery>();
    }

    public string OpenReferralOrganisationId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? Accreditations { get; set; }
    public DateTime? Assured_date { get; set; }
    public string? Attending_access { get; set; }
    public string? Attending_type { get; set; }
    public string? Deliverable_type { get; set; }
    public string? Status { get; set; }
    public string? Url { get; set; }
    public string? Email { get; set; }
    public string? Fees { get; set; }
    public virtual ICollection<OpenReferralServiceDelivery> ServiceDelivery { get; init; } = new Collection<OpenReferralServiceDelivery>();
    public virtual ICollection<OpenReferralEligibility> Eligibilitys { get; init; } = new Collection<OpenReferralEligibility>();
    public virtual ICollection<OpenReferralFunding> Fundings { get; init; } = new Collection<OpenReferralFunding>();
    public virtual ICollection<OpenReferralHoliday_Schedule> Holiday_schedules { get; init; } = new Collection<OpenReferralHoliday_Schedule>();
    public virtual ICollection<OpenReferralLanguage> Languages { get; init; } = new Collection<OpenReferralLanguage>();
    public virtual ICollection<OpenReferralRegular_Schedule> Regular_schedules { get; init; } = new Collection<OpenReferralRegular_Schedule>();
    public virtual ICollection<OpenReferralReview> Reviews { get; init; } = new Collection<OpenReferralReview>();
    public virtual ICollection<OpenReferralContact> Contacts { get; init; } = new Collection<OpenReferralContact>();
    public virtual ICollection<OpenReferralCost_Option> Cost_options { get; init; } = new Collection<OpenReferralCost_Option>();
    public virtual ICollection<OpenReferralService_Area> Service_areas { get; init; } = new Collection<OpenReferralService_Area>();
    public virtual ICollection<OpenReferralServiceAtLocation> Service_at_locations { get; init; } = new Collection<OpenReferralServiceAtLocation>();
    public virtual ICollection<OpenReferralService_Taxonomy> Service_taxonomys { get; init; } = new Collection<OpenReferralService_Taxonomy>();

    public void Update(OpenReferralService openReferralService)
    {
        Name = openReferralService.Name;
        Description = openReferralService.Description;
        Accreditations = openReferralService.Accreditations;
        Assured_date = openReferralService.Assured_date;
        Attending_access = openReferralService.Attending_access;
        Attending_type = openReferralService.Attending_type;
        Deliverable_type = openReferralService.Deliverable_type;
        Status = openReferralService.Status;
        Url = openReferralService.Url;
        Email = openReferralService.Email;
        Fees = openReferralService.Fees;
    }
}
