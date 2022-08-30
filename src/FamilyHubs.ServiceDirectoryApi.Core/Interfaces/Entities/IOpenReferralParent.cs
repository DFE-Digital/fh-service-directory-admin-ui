using fh_service_directory_api.core.Entities;

namespace fh_service_directory_api.core.Interfaces.Entities
{
    public interface IOpenReferralParent : IEntityBase<string>
    {
        ICollection<OpenReferralLinktaxonomycollection>? LinkTaxonomyCollection { get; init; }
        string Name { get; init; }
        ICollection<OpenReferralService_Taxonomy>? ServiceTaxonomyCollection { get; init; }
        string? Vocabulary { get; init; }
    }
}