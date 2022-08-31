using FamilyHubs.SharedKernel;
using FamilyHubs.SharedKernel.Interfaces;
using fh_service_directory_api.core.Interfaces.Entities;

namespace fh_service_directory_api.core.Entities;

public class OpenReferralCost_Option : EntityBase<string>, IOpenReferralCost_Option, IAggregateRoot
{
    private OpenReferralCost_Option() { }
    public OpenReferralCost_Option(string id, string amount_description, decimal amount, string? linkId, string? option, DateTime? valid_from
        , DateTime? valid_to
        )
    {
        Id = id;
        Amount_description = amount_description;
        Amount = amount;
        LinkId = linkId;
        Option = option;
        Valid_from = valid_from;
        Valid_to = valid_to;
    }
    public string Amount_description { get; init; } = default!;
    public decimal Amount { get; init; }
    public string? LinkId { get; init; }
    public string? Option { get; init; }
    public DateTime? Valid_from { get; init; }
    public DateTime? Valid_to { get; init; }
}
