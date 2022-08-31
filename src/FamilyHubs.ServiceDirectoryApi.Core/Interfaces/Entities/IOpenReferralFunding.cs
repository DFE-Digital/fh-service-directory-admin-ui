namespace fh_service_directory_api.core.Interfaces.Entities
{
    public interface IOpenReferralFunding : IEntityBase<string>
    {
        string Source { get; init; }
    }
}