using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Razor.Dashboard;
using FamilyHubs.SharedKernel.Razor.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.La;

public enum Column
{
    ContactInFamily,
    Service,
    DateUpdated,
    DateSent,
    RequestNumber,
    Status,
    Last = Status
}


[Authorize(Roles = $"{RoleTypes.LaDualRole},{RoleTypes.LaManager}")]
public class SubjectAccessResultDetailsModel : PageModel, IDashboard<ReferralDto>
{
    private readonly IRequestDistributedCache _requestCache;
    private readonly IReferralService _referralService;
    private readonly IConfiguration _configuration;

    private static ColumnImmutable[] _columnImmutables =
    {
        new("Contact in family", Column.ContactInFamily.ToString()),
        new("Service", Column.Service.ToString()),
        new("Date updated", Column.DateUpdated.ToString()),
        new("Date sent", Column.DateSent.ToString()),
        new("Request number"),
        new("Status", Column.Status.ToString())
    };

    private IEnumerable<IColumnHeader> _columnHeaders = Enumerable.Empty<IColumnHeader>();
    private IEnumerable<IRow<ReferralDto>> _rows = Enumerable.Empty<IRow<ReferralDto>>();

    IEnumerable<IColumnHeader> IDashboard<ReferralDto>.ColumnHeaders => _columnHeaders;
    IEnumerable<IRow<ReferralDto>> IDashboard<ReferralDto>.Rows => _rows;

    public List<ReferralDto> ReferralDtos { get; private set; } = new List<ReferralDto>();

    public string? TableClass => "app-la-dashboard";

    public IPagination Pagination { get; set; }

    public SubjectAccessResultDetailsModel(IRequestDistributedCache requestCache, IReferralService referralService, IConfiguration configuration)
    {
        _requestCache = requestCache;
        _referralService = referralService;
        _configuration = configuration;
        Pagination = new DontShowLinkPagination();
    }

    public async Task OnGet(string? columnName, SortOrder sort)
    {
        var user = HttpContext.GetFamilyHubsUser();
        SubjectAccessRequestViewModel? subjectAccessRequestViewModel = await _requestCache.GetSarAsync(user.Email);
        if (subjectAccessRequestViewModel == null)
        {
            return;
        }

        ReferralDtos = await _referralService.GetReferralsByRecipient(subjectAccessRequestViewModel);

        if (columnName == null || !Enum.TryParse(columnName, true, out Column column))
        {
            // default when first load the page, or user has manually changed the url
            column = Column.DateUpdated;
            sort = SortOrder.descending;
        }

        _columnHeaders = new ColumnHeaderFactory(_columnImmutables, "/SubjectAccessResultDetails", column.ToString(), sort)
            .CreateAll();

        string url = _configuration["GovUkOidcConfiguration:AppHost"]!;

        _rows = ReferralDtos.Select(r => new LaDashboardRow(r, new Uri(url)));
    }
}
