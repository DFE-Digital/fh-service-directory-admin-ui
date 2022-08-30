//using fh_service_directory_api.core.Interfaces.Entities;
//using LocalAuthorityInformationServices.SharedKernel;

//namespace fh_service_directory_api.core.Entities;

//public class Eligibility : EntityBase<string>, IEligibility
//{
//    private Eligibility() { }

//    public Eligibility
//    (
//        string id,
//        string eligibility,
//        string? linkId,
//        int maximum_age,
//        int minimum_age,
//        ICollection<ITaxonomy>? taxonomys
//    )
//    {
//        Id = id;
//        _Eligibility = eligibility;
//        LinkId = linkId;
//        Maximum_age = maximum_age;
//        Minimum_age = minimum_age;
//        taxonomys = Taxonomys;
//    }
//    public string _Eligibility { get; init; } = default!;

//    public string? LinkId { get; init; }

//    public int Maximum_age { get; init; }

//    public int Minimum_age { get; init; }

//    public virtual ICollection<ITaxonomy>? Taxonomys { get; init; }
//}
