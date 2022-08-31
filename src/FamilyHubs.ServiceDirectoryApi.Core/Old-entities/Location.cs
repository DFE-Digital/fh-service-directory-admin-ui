//using fh_service_directory_api.core.Interfaces.Entities;
//using LocalAuthorityInformationServices.SharedKernel;

//namespace fh_service_directory_api.core.Entities;

//public class Location : EntityBase<string>, ILocation
//{
//    private Location() { }

//    public Location
//    (
//        string id,
//        string name,
//        string? description,
//        double latitude,
//        double longitude,
//        ICollection<IPhysicalAddress>? physical_addresses,
//        ICollection<IAccessibilityForDisabilities>? accessibility_for_disabilities,
//        ICollection<ServiceAtLocation>? service_at_locations
//    )
//    {
//        Id = id;
//        Name = name;
//        Description = description;
//        Latitude = latitude;
//        Longitude = longitude;
//        PhysicalAddresses = physical_addresses;
//        Accessibility_for_disabilities = accessibility_for_disabilities;
//        Service_at_locations = service_at_locations;
//    }
//    public string Name { get; init; } = default!;

//    public string? Description { get; init; }

//    public double Latitude { get; init; }

//    public double Longitude { get; init; }

//    public virtual ICollection<IPhysicalAddress>? PhysicalAddresses { get; init; }

//    public virtual ICollection<IAccessibilityForDisabilities>? Accessibility_for_disabilities { get; init; }

//    public virtual ICollection<ServiceAtLocation>? Service_at_locations { get; init; }
//}
