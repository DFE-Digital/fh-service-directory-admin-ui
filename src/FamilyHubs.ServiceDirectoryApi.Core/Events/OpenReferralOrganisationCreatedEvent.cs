using FamilyHubs.SharedKernel;
using fh_service_directory_api.core.Entities;
using fh_service_directory_api.core.Interfaces.Entities;
using fh_service_directory_api.core.Interfaces.Events;

namespace fh_service_directory_api.core.Events;

public class OpenReferralOrganisationCreatedEvent : DomainEventBase, IOpenReferralOrganisationCreatedEvent
{
    public OpenReferralOrganisationCreatedEvent(OpenReferralOrganisation item)
    {
        Item = item;
    }

    public OpenReferralOrganisation Item { get; }
}
