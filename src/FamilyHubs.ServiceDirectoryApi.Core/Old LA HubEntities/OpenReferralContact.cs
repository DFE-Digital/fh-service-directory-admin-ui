//using FamilyHubs.SharedKernel;
//using FamilyHubs.SharedKernel.Interfaces;
//using fh_service_directory_api.core.Interfaces.Entities;

//namespace fh_service_directory_api.core.Entities;

//public class OpenReferralContact : EntityBase<string>, IOpenReferralContact, IAggregateRoot
//{
//    private OpenReferralContact() { }
//    public OpenReferralContact(string id, string title, string name, ICollection<OpenReferralPhone>? phones)
//    {
//        Id = id;
//        Title = title;
//        Name = name;
//        Phones = phones;
//    }
//    public string Title { get; init; } = default!;
//    public string Name { get; init; } = default!;
//    public virtual ICollection<OpenReferralPhone>? Phones { get; init; }

//}
