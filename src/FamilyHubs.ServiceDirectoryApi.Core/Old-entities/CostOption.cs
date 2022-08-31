//using fh_service_directory_api.core.Interfaces.Entities;
//using LocalAuthorityInformationServices.SharedKernel;

//namespace fh_service_directory_api.core.Entities;

//public class CostOption : EntityBase<string>, ICostOption
//{
//    private CostOption() { }
//    public CostOption
//    (
//        string id,
//        string amount_description,
//        int amount,
//        string? linkId,
//        string? option,
//        DateTime? valid_from,
//        DateTime? valid_to
//    )
//    {
//        Amount_description = amount_description;
//        Amount = amount;
//        LinkId = linkId;
//        Option = option;
//        Valid_from = valid_from;
//        Valid_to = valid_to;
//    }
//    public string Amount_description { get; init; } = default!;

//    public int Amount { get; init; }

//    public string? LinkId { get; init; }

//    public string? Option { get; init; }

//    public DateTime? Valid_from { get; init; }

//    public DateTime? Valid_to { get; init; }
//}
