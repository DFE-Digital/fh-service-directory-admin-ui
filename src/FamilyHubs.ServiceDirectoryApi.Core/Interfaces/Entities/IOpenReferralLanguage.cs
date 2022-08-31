namespace fh_service_directory_api.core.Interfaces.Entities
{
    public interface IOpenReferralLanguage : IEntityBase<string>
    {
        string Language { get; init; }
    }
}