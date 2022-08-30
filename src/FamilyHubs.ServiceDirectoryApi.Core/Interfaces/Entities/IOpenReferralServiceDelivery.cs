using FamilyHubs.ServiceDirectory.Shared.Enums;

namespace fh_service_directory_api.core.Interfaces.Entities
{
    public interface IOpenReferralServiceDelivery : IEntityBase<string>
    {
        ServiceDelivery ServiceDelivery { get; }
    }
}