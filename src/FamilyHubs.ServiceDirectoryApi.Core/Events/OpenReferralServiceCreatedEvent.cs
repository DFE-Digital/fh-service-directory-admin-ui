using FamilyHubs.SharedKernel;
using fh_service_directory_api.core.Entities;

namespace fh_service_directory_api.core.Events;

public class OpenReferralServiceCreatedEvent : DomainEventBase
{
    public OpenReferralServiceCreatedEvent(OpenReferralService item)
    {
        Item = item;
    }

    public OpenReferralService Item { get; }
}
