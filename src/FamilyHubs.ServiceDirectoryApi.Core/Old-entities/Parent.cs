//using fh_service_directory_api.core.Interfaces.Entities;
//using LocalAuthorityInformationServices.SharedKernel;

//namespace fh_service_directory_api.core.Entities;

//public class Parent : EntityBase<string>, IParent
//{
//    private Parent() { }
//    public Parent(string id, string name, string? vocabulary, ICollection<ITaxonomy>? serviceTaxonomyCollection, ICollection<ILinkTaxonomyCollection>? linkTaxonomyCollection)
//    {
//        Id = id;
//        Name = name;
//        Vocabulary = vocabulary;
//        ServiceTaxonomyCollection = serviceTaxonomyCollection;
//        LinkTaxonomyCollection = (ICollection<LinkTaxonomyCollection>?)linkTaxonomyCollection;
//    }
//    public string Name { get; init; } = default!;

//    public string? Vocabulary { get; init; }

//    public virtual ICollection<ITaxonomy>? ServiceTaxonomyCollection { get; init; }

//    public virtual ICollection<LinkTaxonomyCollection>? LinkTaxonomyCollection { get; init; }
//}
