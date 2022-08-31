//using FamilyHubs.SharedKernel;
//using FamilyHubs.SharedKernel.Interfaces;
//using fh_service_directory_api.core.Interfaces.Entities;

//namespace fh_service_directory_api.core.Entities;

//public class OpenReferralPhysical_Address : EntityBase<string>, IOpenReferralPhysical_Address, IAggregateRoot
//{
//    private OpenReferralPhysical_Address() { }
//    public OpenReferralPhysical_Address(string id, string address_1, string? city, string postal_code, string? country, string? state_province)
//    {
//        Id = id;
//        Address_1 = address_1;
//        City = city;
//        Postal_code = postal_code;
//        Country = country;
//        State_province = state_province;
//    }

//    public string Address_1 { get; init; } = default!;
//    public string? City { get; init; }
//    public string Postal_code { get; init; } = default!;
//    public string? Country { get; init; }
//    public string? State_province { get; init; }
//}
