using FamilyHubs.SharedKernel;
using FamilyHubs.SharedKernel.Interfaces;
using fh_service_directory_api.core.Interfaces.Entities;

namespace fh_service_directory_api.core.Entities;

public class OpenReferralParent : EntityBase<string>, IOpenReferralParent, IAggregateRoot
{
    private OpenReferralParent() { }
    public OpenReferralParent(string id, string name, string? vocabulary, ICollection<OpenReferralService_Taxonomy>? serviceTaxonomyCollection, ICollection<OpenReferralLinktaxonomycollection>? linkTaxonomyCollection)
    {
        Id = id;
        Name = name;
        Vocabulary = vocabulary;
        ServiceTaxonomyCollection = serviceTaxonomyCollection as ICollection<OpenReferralService_Taxonomy>;
        LinkTaxonomyCollection = linkTaxonomyCollection as ICollection<OpenReferralLinktaxonomycollection>;
    }
    public string Name { get; init; } = default!;
    public string? Vocabulary { get; init; }
    public virtual ICollection<OpenReferralService_Taxonomy>? ServiceTaxonomyCollection { get; init; }
    public virtual ICollection<OpenReferralLinktaxonomycollection>? LinkTaxonomyCollection { get; init; }
}
