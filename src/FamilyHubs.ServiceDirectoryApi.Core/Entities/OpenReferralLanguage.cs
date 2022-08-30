using FamilyHubs.SharedKernel;
using FamilyHubs.SharedKernel.Interfaces;
using fh_service_directory_api.core.Interfaces.Entities;

namespace fh_service_directory_api.core.Entities;

public class OpenReferralLanguage : EntityBase<string>, IOpenReferralLanguage, IAggregateRoot
{
    private OpenReferralLanguage() { }
    public OpenReferralLanguage(string id, string language)
    {
        Id = id;
        Language = language;
    }
    public string Language { get; init; } = default!;
}
