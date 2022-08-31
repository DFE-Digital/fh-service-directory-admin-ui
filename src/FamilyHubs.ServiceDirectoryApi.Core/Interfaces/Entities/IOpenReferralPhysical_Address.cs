namespace fh_service_directory_api.core.Interfaces.Entities
{
    public interface IOpenReferralPhysical_Address : IEntityBase<string>
    {
        string Address_1 { get; init; }
        string? City { get; init; }
        string? Country { get; init; }
        string Postal_code { get; init; }
        string? State_province { get; init; }
    }
}