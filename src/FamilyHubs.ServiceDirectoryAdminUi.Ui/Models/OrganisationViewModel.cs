namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;

public class OrganisationViewModel
{

    public Guid Id { get; set; }
    public string? Name { get; set; } = default!;
    public string? Type { get; set; } = default!;
    public string? Description { get; set; }
    public string? Logo { get; set; }
    public string? Uri { get; set; }
    public string? Url { get; set; }
    public string? ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public string? ServiceType { get; set; }
    public string? ServiceDescription { get; set; }
    public List<string>? TaxonomySelection { get; set; } = default!;
    public List<string>? ServiceDeliverySelection { get; set; } = default!;
    public string? Familychoice { get; set; } = default!;
    public string? Children { get; set; } = default!;
    public List<string>? InPersonSelection { get; set; } = default!;
    public List<string>? WhoForSelection { get; set; } = default!;
    public List<string>? Languages { get; set; } = default!;
    public int? MinAge { get; set; } = default!;
    public int? MaxAge { get; set; } = default!;
    public string? LocationName { get; set; } = default!;
    public string? LocationDescription { get; set; } = default!;
    public string? Address_1 { get; set; } = default!;
    public string? City { get; set; } = default!;
    public string? Postal_code { get; set; } = default!;
    public double? Latitude { get; set; } = default!;
    public double? Longtitude { get; set; } = default!;
    public string? Country { get; set; } = default!;
    public string? State_province { get; set; } = default!;
    public List<string>? RegularSchedules { get; set; } = default!;
    public string? IsPayedFor { get; set; } = default!;
    public string? PayUnit { get; set; } = default!;
    public decimal? Cost { get; set; } = default!;
    public List<string>? ContactSelection { get; set; } = default!;
    public string? Email { get; set; } = default!;
    public string? Telephone { get; set; } = default!;
    public string? Website { get; set; } = default!;
    public string? Textphone { get; set; } = default!;

}