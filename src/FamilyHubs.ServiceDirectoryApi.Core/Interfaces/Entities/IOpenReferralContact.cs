using fh_service_directory_api.core.Entities;

namespace fh_service_directory_api.core.Interfaces.Entities;

public interface IOpenReferralContact : IEntityBase<string>
{
    string Name { get; init; }
    ICollection<OpenReferralPhone>? Phones { get; init; }
    string Title { get; init; }
}