using fh_service_directory_api.core.Entities;

namespace fh_service_directory_api.core.Interfaces.Entities
{
    public interface IOpenReferralService_Taxonomy : IEntityBase<string>
    {
        string? LinkId { get; init; }
        OpenReferralTaxonomy? Taxonomy { get; set; }
    }
}