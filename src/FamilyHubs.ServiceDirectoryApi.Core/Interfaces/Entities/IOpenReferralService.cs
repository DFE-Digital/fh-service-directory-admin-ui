using fh_service_directory_api.core.Entities;

namespace fh_service_directory_api.core.Interfaces.Entities
{
    public interface IOpenReferralService : IEntityBase<string>
    {
        string? Accreditations { get; }
        DateTime? Assured_date { get; }
        string? Attending_access { get; }
        string? Attending_type { get; }
        ICollection<OpenReferralContact> Contacts { get; init; }
        ICollection<OpenReferralCost_Option> Cost_options { get; init; }
        string? Deliverable_type { get; }
        string? Description { get; }
        ICollection<OpenReferralEligibility> Eligibilitys { get; init; }
        string? Email { get; }
        string? Fees { get; }
        ICollection<OpenReferralFunding> Fundings { get; init; }
        ICollection<OpenReferralHoliday_Schedule> Holiday_schedules { get; init; }
        ICollection<OpenReferralLanguage> Languages { get; init; }
        string Name { get; }
        string OpenReferralOrganisationId { get; set; }
        ICollection<OpenReferralRegular_Schedule> Regular_schedules { get; init; }
        ICollection<OpenReferralReview> Reviews { get; init; }
        ICollection<OpenReferralService_Area> Service_areas { get; init; }
        ICollection<OpenReferralServiceAtLocation> Service_at_locations { get; init; }
        ICollection<OpenReferralService_Taxonomy> Service_taxonomys { get; init; }
        ICollection<OpenReferralServiceDelivery> ServiceDelivery { get; init; }
        string? Status { get; }
        string? Url { get; }

        void Update(OpenReferralService openReferralService);
    }
}