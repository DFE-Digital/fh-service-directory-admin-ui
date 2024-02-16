﻿
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;

namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

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
    //todo: is this necessary, can we just check TimeDescription != null?
    public bool? HasTimeDetails { get; set; }
    public string? TimeDescription { get; set; }
    public IEnumerable<string>? Times { get; set; }
    public string? MoreDetails { get; set; }

    //todo: temporary, until we have a service at location to store it
    public IEnumerable<string>? ServiceAtLocationTimes { get; set; }
    public ServiceDeliveryType[] HowUse { get; set; } = Array.Empty<ServiceDeliveryType>();
    public bool? AddLocations { get; set; }
}