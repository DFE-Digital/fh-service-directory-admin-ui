namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

public class LocationModel : LocationModel<object>
{
}

public class LocationModel<TUserInput>
    : JourneyCacheModel<LocationJourneyPage, ErrorId, TUserInput>
{
    public bool? IsFamilyHub { get; set; }
    public string? Description { get; set; }
}
