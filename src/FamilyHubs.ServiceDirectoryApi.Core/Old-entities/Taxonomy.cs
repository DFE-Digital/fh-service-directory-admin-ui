//using fh_service_directory_api.core.Interfaces.Entities;
//using LocalAuthorityInformationServices.SharedKernel;

//namespace fh_service_directory_api.core.Entities;

//public class Taxonomy : EntityBase<string>, ITaxonomy
//{
//    private Taxonomy() { }
//    public Taxonomy
//    (
//        string id,
//        string name,
//        string? vocabulary,
//        string? parent,
//        ICollection<LinkTaxonomyCollection>? linkTaxonomyCollection,
//        ICollection<Taxonomy>? serviceTaxonomyCollection
//    )
//    {
//        Id = id;
//        LinkTaxonomyCollection = linkTaxonomyCollection;
//        Name = name;
//        ServiceTaxonomyCollection = serviceTaxonomyCollection;
//        Vocabulary = vocabulary;
//        Parent = parent;

//    }

//    public string Name { get; init; } = default!;

//    public string? Vocabulary { get; init; }

//    public string? Parent { get; init; }

//    public ICollection<LinkTaxonomyCollection>? LinkTaxonomyCollection { get; init; }

//    public ICollection<Taxonomy>? ServiceTaxonomyCollection { get; init; }
//}
