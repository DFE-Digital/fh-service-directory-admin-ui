﻿
namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

public class ServiceModel : ServiceModel<object>
{
}

public class ServiceModel<T>
{
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
    public TimeModel? WeekdaysStarts { get; set; }
    public TimeModel? WeekdaysFinishes { get; set; }
    public TimeModel? WeekendsStarts { get; set; }
    public TimeModel? WeekendsFinishes { get; set; }

    public ServiceErrorState? ErrorState { get; set; }

    public string? UserInputType { get; set; }
    public T? UserInput { get; set; }
}