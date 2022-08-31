using FamilyHubs.SharedKernel;
using fh_service_directory_api.core.Entities;
using fh_service_directory_api.core.Interfaces.Entities;

namespace fh_service_directory_api.core.Events;

public class OpenReferralReviewCreatedEvent : DomainEventBase
{
    public OpenReferralReviewCreatedEvent(OpenReferralReview item)
    {
        Item = item;
    }

    public OpenReferralReview Item { get; }
}