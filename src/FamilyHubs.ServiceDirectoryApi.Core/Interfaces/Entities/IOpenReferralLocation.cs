using fh_service_directory_api.core.Entities;

namespace fh_service_directory_api.core.Interfaces.Entities
{
    public interface IOpenReferralLocation : IEntityBase<string>
    {
        ICollection<Accessibility_For_Disabilities>? Accessibility_for_disabilities { get; init; }
        string? Description { get; init; }
        double Latitude { get; init; }
        double Longitude { get; init; }
        string Name { get; init; }
        ICollection<OpenReferralPhysical_Address>? Physical_addresses { get; init; }
    }
}