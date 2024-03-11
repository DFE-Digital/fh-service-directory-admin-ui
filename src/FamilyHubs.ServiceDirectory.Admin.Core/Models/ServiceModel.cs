using System.Text.Json.Serialization;
using FamilyHubs.ServiceDirectory.Shared.Display;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;

namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

public class ServiceLocationModel
{
    [JsonConstructor]
    public ServiceLocationModel(long id, IEnumerable<string> address, string displayName, bool isFamilyHub, string? description)
    {
        Id = id;
        Address = address;
        DisplayName = displayName;
        IsFamilyHub = isFamilyHub;
        Description = description;
        Times = Enumerable.Empty<string>();
    }

    public ServiceLocationModel(LocationDto location)
    {
        Id = location.Id;
        DisplayName = location.Name ?? string.Join(", ", location.Address1, location.Address2);
        Address = location.GetAddress();
        Description = location.Description;
        //todo: store yes/no?
        IsFamilyHub = location.LocationTypeCategory == LocationTypeCategory.FamilyHub;
        //todo: will need to get this from the schedule off service at location
        Times = Enumerable.Empty<string>();
    }

    public long Id { get; }
    public IEnumerable<string> Address { get; }
    // have this as well?
    public string DisplayName { get; }
    public bool IsFamilyHub { get; }
    public string? Description { get; }
    public IEnumerable<string> Times { get; set; }
    public bool? HasTimeDetails { get; set; }
    public string? TimeDescription { get; set; }
}

public class ServiceModel : ServiceModel<object>
{
}

public class ServiceModel<TUserInput>
    : JourneyCacheModel<ServiceJourneyPage, ErrorId, TUserInput>
    where TUserInput : class?
{
    public long? Id { get; set; }
    //todo: do we want bools to be nullable?
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? ForChildren { get; set; }
    public int? MinimumAge { get; set; }
    public int? MaximumAge { get; set; }
    //todo: remove Selected? nullable, rather than new()?
    public List<long?> SelectedCategories { get; set; } = new();
    public List<long> SelectedSubCategories { get; set; } = new();
    public IEnumerable<string>? LanguageCodes { get; set; }
    public bool? TranslationServices { get; set; }
    public bool? BritishSignLanguage { get; set; }
    public bool? HasCost { get; set; }
    public string? CostDescription { get; set; }
    public bool? HasTimeDetails { get; set; }
    public string? TimeDescription { get; set; }
    public IEnumerable<string>? Times { get; set; }
    public string? MoreDetails { get; set; }
    public AttendingType[] HowUse { get; set; } = Array.Empty<AttendingType>();
    public bool? AddingLocations { get; set; }
    // we _could_ store locations, rather than just ids, as we need to get them from the sd-api occasionally
    // but, then we'd miss if someone else updated the location in the meantime, and it would be unnecessary work for pages that don't need the location
    // although, we reserve the right to change our minds
    public ServiceLocationModel? CurrentLocation { get; set; }
    public List<ServiceLocationModel> Locations { get; set; } = new();
    //public long? OperationLocationId { get; set; }
    public string? Email { get; set; }
    public bool HasEmail { get; set; }
    public string? TelephoneNumber { get; set; }
    public bool HasTelephone { get; set; }
    public string? Website { get; set; }
    public bool HasWebsite { get; set; }
    public string? TextTelephoneNumber { get; set; }
    public bool HasTextMessage { get; set; }

    public ServiceLocationModel GetLocation(long locationId)
    {
        if (CurrentLocation?.Id == locationId)
            return CurrentLocation;

        return Locations.Single(l => l.Id == locationId);
    }
}