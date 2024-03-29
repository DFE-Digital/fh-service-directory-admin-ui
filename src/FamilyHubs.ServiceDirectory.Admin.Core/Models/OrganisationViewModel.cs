﻿namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

public class OrganisationViewModel
{
    public long Id { get; init; }
    public string? Name { get; set; }
    public string? Type { get; init; }
    public string? Description { get; init; }
    public string? Logo { get; init; }
    public string? Uri { get; init; }
    public string? Url { get; init; }
    public long? ServiceId { get; set; }
    public string? ServiceOwnerReferenceId { get; set; }
    public string? ServiceName { get; set; }
    public string? ServiceType { get; set; }
    public string? ServiceDescription { get; set; }
    public List<long>? TaxonomySelection { get; set; }
    public List<string>? ServiceDeliverySelection { get; set; }
    public string? FamilyChoice { get; set; }
    public string? Children { get; set; }
    public List<string>? InPersonSelection { get; set; }
    public List<string>? WhoForSelection { get; set; }
    public List<string>? Languages { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public string? LocationName { get; set; }
    public string? LocationDescription { get; set; }
    public string? Address1 { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Country { get; set; }
    public string? StateProvince { get; set; }
    public List<string>? RegularSchedules { get; set; }
    public string? IsPayedFor { get; set; }
    public string? PayUnit { get; set; }
    public decimal? Cost { get; set; }
    public List<string>? CostDescriptions { get; set; }
    public List<string>? ContactSelection { get; set; }
    public string? CostDetails { get; set; }
    public string? Email { get; set; }
    public string? Telephone { get; set; }
    public string? Website { get; set; }
    public string? TextPhone { get; set; }
    public bool? HasSetDaysAndTimes { get; set; }
    public List<string> DaySelection { get; set; } = default!;
    public bool? IsSameTimeOnEachDay { get; set; }
    public List<OpeningHours> OpeningHours { get; set; } = default!;

    public static int GetDaySort(string? day)
    {
        if (day == null) throw new ArgumentNullException(nameof(day));

        if (Char.IsDigit(day[0]))
        {
            int.TryParse(day, out int daynumber);
            return daynumber;
        }

        switch (day.ToLower())
        {
            case "monday":
                return 1;
            case "tuesday":
                return 2;
            case "wednesday":
                return 3;
            case "thursday":
                return 4;
            case "friday":
                return 5;
            case "saturday":
                return 6;
        }

        return 7;
    }
}

public class OpeningHours
{
    public int SortOrder { get { return OrganisationViewModel.GetDaySort(this.Day); } }
    public string? Day { get; set; }
    public string? StartsTimeOfDay { get; set; }
    public string? Starts { get; set; }
    public string? FinishesTimeOfDay { get; set; }
    public string? Finishes { get; set; }

}