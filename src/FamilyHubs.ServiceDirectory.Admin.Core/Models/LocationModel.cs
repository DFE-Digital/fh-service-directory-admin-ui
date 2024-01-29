namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

public class LocationModel : LocationModel<object>
{

}

public class LocationModel<TUserInput>
    : JourneyCacheModel<LocationJourneyPage, ErrorId, TUserInput>
{
    public bool? IsFamilyHub { get; set; }
    public string? Description { get; set; }
    public string? BuildingName { get; set; } = string.Empty;
    public string? Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; } = string.Empty;
    public string? TownOrCity { get; set; } = string.Empty;
    public string? County { get; set; } = string.Empty;
    public string? Postcode { get; set; } = string.Empty;
}
