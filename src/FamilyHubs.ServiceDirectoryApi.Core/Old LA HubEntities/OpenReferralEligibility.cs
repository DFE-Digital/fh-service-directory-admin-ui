//using FamilyHubs.SharedKernel;
//using FamilyHubs.SharedKernel.Interfaces;
//using fh_service_directory_api.core.Interfaces.Entities;

//namespace fh_service_directory_api.core.Entities;

//public class OpenReferralEligibility : EntityBase<string>, IOpenReferralEligibility, IAggregateRoot
//{
//    private OpenReferralEligibility() { }
//    public OpenReferralEligibility(string id, string eligibility, string? linkId, int maximum_age, int minimum_age, ICollection<OpenReferralTaxonomy>? taxonomys)
//    {
//        Id = id;
//        Eligibility = eligibility;
//        LinkId = linkId;
//        Maximum_age = maximum_age;
//        Minimum_age = minimum_age;
//        taxonomys = Taxonomys;
//    }
//    public string Eligibility { get; init; } = default!;
//    public string? LinkId { get; init; }
//    public int Maximum_age { get; init; }
//    public int Minimum_age { get; init; }
//    public virtual ICollection<OpenReferralTaxonomy>? Taxonomys { get; init; }
//}
