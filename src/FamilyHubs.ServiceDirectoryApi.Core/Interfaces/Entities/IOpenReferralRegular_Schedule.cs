namespace fh_service_directory_api.core.Interfaces.Entities
{
    public interface IOpenReferralRegular_Schedule : IEntityBase<string>
    {
        string? Byday { get; init; }
        string? Bymonthday { get; init; }
        DateTime? Closes_at { get; init; }
        string Description { get; init; }
        string? Dtstart { get; init; }
        string? Freq { get; init; }
        string? Interval { get; init; }
        DateTime? Opens_at { get; init; }
        DateTime? Valid_from { get; init; }
        DateTime? Valid_to { get; init; }
    }
}