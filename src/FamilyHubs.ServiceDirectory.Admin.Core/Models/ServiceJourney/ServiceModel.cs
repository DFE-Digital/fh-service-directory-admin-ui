﻿using FamilyHubs.ServiceDirectory.Shared.Enums;

namespace FamilyHubs.ServiceDirectory.Admin.Core.Models.ServiceJourney;

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
    public ServiceLocationModel? CurrentLocation { get; set; }
    public List<ServiceLocationModel> Locations { get; set; } = new();
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

    public IEnumerable<ServiceLocationModel> AllLocations
    {
        get
        {
            if (CurrentLocation != null)
            {
                yield return CurrentLocation;
            }

            foreach (var location in Locations)
            {
                yield return location;
            }
        }
    }

    public void RemoveAllLocations()
    {
        AddingLocations = null;
        CurrentLocation = null;
        Locations.Clear();
    }
}