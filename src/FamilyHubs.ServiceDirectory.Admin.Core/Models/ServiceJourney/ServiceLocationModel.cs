using System.Text.Json.Serialization;
using FamilyHubs.ServiceDirectory.Shared.Display;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;

namespace FamilyHubs.ServiceDirectory.Admin.Core.Models.ServiceJourney;

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
    }

    public ServiceLocationModel(LocationDto location)
    {
        Id = location.Id;
        DisplayName = location.GetDisplayName();
        Address = location.GetAddress();
        Description = location.Description;
        //todo: store yes/no?
        IsFamilyHub = location.LocationTypeCategory == LocationTypeCategory.FamilyHub;
    }

    public long Id { get; }
    public IEnumerable<string> Address { get; }
    // have this as well?
    public string DisplayName { get; }
    public bool IsFamilyHub { get; }
    public string? Description { get; }
    public IEnumerable<string>? Times { get; set; }
    public bool? HasTimeDetails { get; set; }
    public string? TimeDescription { get; set; }
}
