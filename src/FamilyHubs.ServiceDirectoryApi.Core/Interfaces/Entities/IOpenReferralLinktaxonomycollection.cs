namespace fh_service_directory_api.core.Interfaces.Entities
{
    public interface IOpenReferralLinktaxonomycollection : IEntityBase<string>
    {
        string Link_id { get; init; }
        string Link_type { get; init; }
    }
}