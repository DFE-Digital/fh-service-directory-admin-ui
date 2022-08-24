namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Models
{
    public interface IOrganisationViewModel
    {
        string? Address_1 { get; set; }
        string? City { get; set; }
        List<string>? ContactSelection { get; set; }
        decimal? Cost { get; set; }
        string? Country { get; set; }
        string? Description { get; set; }
        string? Email { get; set; }
        Guid Id { get; set; }
        List<string>? InPersonSelection { get; set; }
        string? IsPayedFor { get; set; }
        List<string>? Languages { get; set; }
        double? Latitude { get; set; }
        string? Logo { get; set; }
        double? Longtitude { get; set; }
        int? MaxAge { get; set; }
        int? MinAge { get; set; }
        string? Name { get; set; }
        string? PayUnit { get; set; }
        string? Postal_code { get; set; }
        List<string>? ServiceDeliverySelection { get; set; }
        string? ServiceDescription { get; set; }
        string? ServiceId { get; set; }
        string? ServiceName { get; set; }
        string? State_province { get; set; }
        List<string>? TaxonomySelection { get; set; }
        string? Telephone { get; set; }
        string? Uri { get; set; }
        string? Url { get; set; }
        string? Website { get; set; }
        List<string>? WhoForSelection { get; set; }
    }
}