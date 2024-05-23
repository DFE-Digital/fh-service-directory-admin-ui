using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Reports.WeeklyBreakdown;
using Microsoft.AspNetCore.Authorization;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.performance_data;

[Authorize(Roles = $"{RoleTypes.DfeAdmin},{RoleGroups.LaManagerOrDualRole}")]
public class FindPerformanceDataModel : HeaderPageModel
{
    public string Title => "Performance data for Find support for your family";
    public string? OrgName { get; private set; }
    public Dictionary<PerformanceDataType, long> Totals { get; private set; } = new();
    public WeeklyReportBreakdown Breakdown { get; private set; } = new();

    private readonly IServiceDirectoryClient _serviceDirectoryClient;
    private readonly IReportingClient _reportingClient;

    public ReportingNavigationDataModel NavigationDataModel { get; private set; } = new()
    {
        ActivePage = ReportingNavigationDataModel.Page.Find
    };

    public FindPerformanceDataModel(IServiceDirectoryClient serviceDirectoryClient, IReportingClient reportingClient)
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
        if (user.Role != RoleTypes.DfeAdmin)
        {
            organisationId = long.Parse(user.OrganisationId);
            organisation = await _serviceDirectoryClient.GetOrganisationById(organisationId.Value, cancellationToken);
        }

        OrgName = organisation?.Name;

        var searches = await _reportingClient.GetServicesSearchesTotal(ServiceType.InformationSharing, organisationId, cancellationToken);
        var searchesPast7Days = await _reportingClient.GetServicesSearchesPast7Days(ServiceType.InformationSharing, organisationId, cancellationToken);

        Totals = new Dictionary<PerformanceDataType, long>
        {
            { PerformanceDataType.SearchesTotal, searches },
            { PerformanceDataType.SearchesLast7Days, searchesPast7Days }
        };

        Breakdown = await _reportingClient.GetServicesSearches4WeekBreakdown(ServiceType.InformationSharing, organisationId, cancellationToken);
    }
}
