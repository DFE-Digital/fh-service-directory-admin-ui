namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

public class LocationModel : LocationModel<object>
{
}

public class LocationModel<TUserInput>
    : JourneyCacheModel<LocationJourneyPage, ErrorId, TUserInput>
    where TUserInput : class?
{
    public long? Id { get; set; }
    public bool? IsFamilyHub { get; set; }
    public string? Description { get; set; }
    //todo: work with null?
    public string? Name { get; set; } = string.Empty;
    public string? AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; } = string.Empty;
    public string? City { get; set; } = string.Empty;
    public string? County { get; set; } = string.Empty;
    public string? Postcode { get; set; } = string.Empty;
    public float? Latitude { get; set; }
    public float? Longitude { get; set; }
}
