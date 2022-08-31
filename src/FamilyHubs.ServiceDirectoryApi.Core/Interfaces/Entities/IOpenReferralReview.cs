using fh_service_directory_api.core.Entities;

namespace fh_service_directory_api.core.Interfaces.Entities
{
    public interface IOpenReferralReview : IEntityBase<string>
    {
        DateTime Date { get; }
        string? Description { get; }
        string? Score { get; }
        string Title { get; }
        string? Url { get; }
        string? Widget { get; }
        string OpenReferralServiceId { get; }
        void Update(OpenReferralReview openReferralReview);
    }
}