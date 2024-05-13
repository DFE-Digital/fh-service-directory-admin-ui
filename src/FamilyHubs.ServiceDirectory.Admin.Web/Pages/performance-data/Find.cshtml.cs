using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Authorization;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.performance_data;

[Authorize(Roles = $"{RoleTypes.DfeAdmin},{RoleGroups.LaManagerOrDualRole}")]
public class FindPerformanceDataModel : HeaderPageModel
{
    public const string PagePath = "/performance-data";

    public string Title => "Performance data for Find support for your family";
    public string? OrgName { get; private set; }
    public Dictionary<PerformanceDataType, long> Totals { get; private set; } = new();
    public FourWeekBreakdownDto Breakdown { get; private set; } = new();

    private readonly IServiceDirectoryClient _serviceDirectoryClient;
    private readonly IReportingClient _reportingClient;
    public ReportingNavigationDataModel NavigationDataModel { get; private set; } = new();

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
        OrganisationWithServicesDto? organisation = null;
        NavigationDataModel.IsDfeAdmin = user.Role == RoleTypes.DfeAdmin;
        if (user.Role != RoleTypes.DfeAdmin)
        {
            organisationId = long.Parse(user.OrganisationId);
            organisation = await _serviceDirectoryClient.GetOrganisationById(organisationId.Value, cancellationToken);
        }

        OrgName = organisation?.Name;

        var services = await _serviceDirectoryClient.GetServiceSummaries(organisationId, pageSize: 1, cancellationToken: cancellationToken);
        var searches = await _reportingClient.GetServicesSearchesTotal(organisationId, cancellationToken);
        var searchesPast7Days = await _reportingClient.GetServicesSearchesPast7Days(organisationId, cancellationToken);

        Totals = new Dictionary<PerformanceDataType, long>
        {
            { PerformanceDataType.LocalServices, services.TotalCount },
            { PerformanceDataType.SearchesTotal, searches },
            { PerformanceDataType.SearchesLast7Days, searchesPast7Days }
        };

        if (organisationId != null)
            Breakdown = await _reportingClient.GetServicesSearches4WeekBreakdown(organisationId, cancellationToken);
    }
}

public class PerformanceDataType
{
    public string Name { get; private set; }
    private PerformanceDataType(string name)
    {
        Name = name;
    }

    public static readonly PerformanceDataType LocalServices = new("Local authority services");
    public static readonly PerformanceDataType SearchesTotal = new("Searches");
    public static readonly PerformanceDataType SearchesLast7Days = new("Searches in the last 7 days");
}
