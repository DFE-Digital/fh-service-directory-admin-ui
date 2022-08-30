namespace fh_service_directory_api.core.Interfaces.Entities
{
    public interface IOpenReferralService_Area : IEntityBase<string>
    {
        string? Extent { get; init; }
        string? LinkId { get; init; }
        string Service_area { get; init; }
        string? Uri { get; init; }
    }
}