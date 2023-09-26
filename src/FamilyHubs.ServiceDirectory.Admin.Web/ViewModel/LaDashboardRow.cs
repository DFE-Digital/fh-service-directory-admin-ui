using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.SharedKernel.Razor.Dashboard;
using System.Web;

namespace FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;

public class LaDashboardRow : IRow<ReferralDto>
{
    private readonly Uri _laWebBaseUrl;
    public ReferralDto Item { get; }

    public LaDashboardRow(ReferralDto referral, Uri laWebBaseUrl)
    {
        _laWebBaseUrl = laWebBaseUrl;
        Item = referral;
    }

    public IEnumerable<ICell> Cells
    {
        get
        {
            var requestDetailsUrl = $"{_laWebBaseUrl}La/RequestDetails?id={Item.Id}";
            yield return new Cell($"<a href=\"{requestDetailsUrl}\">{HttpUtility.HtmlEncode(Item.RecipientDto.Name)}</a>");
            yield return new Cell(Item.ReferralServiceDto.Name);
            yield return new Cell(Item.LastModified?.ToString("dd MMM yyyy") ?? "");
            yield return new Cell(Item.Created?.ToString("dd MMM yyyy") ?? "");
            yield return new Cell(Item.Id.ToString("X6"));
            yield return new Cell(null, "_LaConnectionStatus");
        }
    }
}
