using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Reports.ConnectionRequests;
using FamilyHubs.SharedKernel.Reports.WeeklyBreakdown;
using Microsoft.AspNetCore.Authorization;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.performance_data;

[Authorize(Roles = $"{RoleTypes.DfeAdmin},{RoleGroups.LaManagerOrDualRole},{RoleGroups.VcsManagerOrDualRole}")]
public class ConnectPerformanceDataModel : HeaderPageModel
{
    public string Title => "Performance data for Connect families to support";
    public string? OrgName { get; private set; }
    public bool IsVcs { get; private set; }
    public Dictionary<PerformanceDataType, long> Totals { get; private set; } = new();
    public Dictionary<PerformanceDataType, long> TotalsLast7Days { get; private set; } = new();
    public WeeklyReportBreakdown Breakdown { get; private set; } = new();
    public ConnectionRequestsBreakdown RequestBreakdown { get; private set; } = new();

    private readonly IServiceDirectoryClient _serviceDirectoryClient;
    private readonly IReportingClient _reportingClient;
    private const ServiceType ConnectServiceType = ServiceType.InformationSharing;

    public ReportingNavigationDataModel NavigationDataModel { get; private set; } = new()
    {
        ActivePage = ReportingNavigationDataModel.Page.Connect,
    };

    public ConnectPerformanceDataModel(IServiceDirectoryClient serviceDirectoryClient, IReportingClient reportingClient)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
        _reportingClient = reportingClient;
    }

    public async Task OnGetAsync(
        CancellationToken cancellationToken)
    {
        var user = HttpContext.GetFamilyHubsUser();

        long? organisationId = null;
        OrganisationDetailsDto? organisation = null;
        NavigationDataModel.IsDfeAdmin = user.Role == RoleTypes.DfeAdmin;
        IsVcs = user.Role is RoleTypes.VcsManager or RoleTypes.VcsDualRole;
        if (user.Role != RoleTypes.DfeAdmin)
        {
            organisationId = long.Parse(user.OrganisationId);
            organisation = await _serviceDirectoryClient.GetOrganisationById(organisationId.Value, cancellationToken);
        }

        OrgName = organisation?.Name;

        var searches = await _reportingClient.GetServicesSearchesTotal(ConnectServiceType, organisationId, cancellationToken);
        var searchesPast7Days = await _reportingClient.GetServicesSearchesPast7Days(ConnectServiceType, organisationId, cancellationToken);

        var requests = await _reportingClient.GetConnectionRequestsTotal(ConnectServiceType, organisationId, cancellationToken);
        var requestPast7Days = await _reportingClient.GetConnectionRequestsPast7Days(ConnectServiceType, organisationId, cancellationToken);

        var requestsType = IsVcs ? PerformanceDataType.ConnectionRequestsVcs : PerformanceDataType.ConnectionRequests;
        Totals = new Dictionary<PerformanceDataType, long>
        {
            { PerformanceDataType.SearchesTotal, searches },
            { requestsType, requests.Made }
        };

        TotalsLast7Days = new Dictionary<PerformanceDataType, long>
        {
            { PerformanceDataType.SearchesTotal, searchesPast7Days },
            { requestsType, requestPast7Days.Made }
        };

        Breakdown = await _reportingClient.GetServicesSearches4WeekBreakdown(ConnectServiceType, organisationId, cancellationToken);
        RequestBreakdown = await _reportingClient.GetConnectionRequests4WeekBreakdown(ConnectServiceType, organisationId, cancellationToken);
    }
}
