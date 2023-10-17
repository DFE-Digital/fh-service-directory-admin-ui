using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.SharedKernel.Razor.Dashboard;
using System.Web;

namespace FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;

public class LaDashboardRow : IRow<ReferralDto>
{
    public ReferralDto Item { get; }

    public LaDashboardRow(ReferralDto referral, Uri laWebBaseUrl)
    {
        Item = referral;
    }

    public IEnumerable<ICell> Cells
    {
        get
        {
            yield return new Cell(HttpUtility.HtmlEncode(Item.RecipientDto.Name));
            yield return new Cell(Item.ReferralServiceDto.Name);
            yield return new Cell(Item.LastModified?.ToString("dd MMM yyyy") ?? "");
            yield return new Cell(Item.Created?.ToString("dd MMM yyyy") ?? "");
            yield return new Cell(Item.Id.ToString("X6"));
            yield return new Cell(null, "_LaConnectionStatus");
        }
    }
}
