namespace fh_service_directory_api.core.Interfaces.Entities
{
    public interface IOpenReferralPhone : IEntityBase<string>
    {
        string Number { get; init; }
    }
}