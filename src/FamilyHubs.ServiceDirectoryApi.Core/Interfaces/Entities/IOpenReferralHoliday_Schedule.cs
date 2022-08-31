namespace fh_service_directory_api.core.Interfaces.Entities
{
    public interface IOpenReferralHoliday_Schedule : IEntityBase<string>
    {
        bool Closed { get; init; }
        DateTime? Closes_at { get; init; }
        DateTime? End_date { get; init; }
        DateTime? Opens_at { get; init; }
        DateTime? Start_date { get; init; }
    }
}