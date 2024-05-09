using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Authorization;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.performance_data;

[Authorize(Roles=RoleGroups.LaManagerOrDualRole)]
public class FindPerformanceDataModel : HeaderPageModel
{
    public const string PagePath = "/performance-data";

    public string Title => "Performance data for Find support for your family";
    public string? OrgName { get; private set; }
    public Dictionary<PerformanceDataType, long> Totals { get; private set; } = new();
    public Dictionary<DateTime, int> Breakdown { get; private set; } = new();

    private readonly IServiceDirectoryClient _serviceDirectoryClient;
    private readonly IReportingClient _reportingClient;

    public FindPerformanceDataModel(IServiceDirectoryClient serviceDirectoryClient, IReportingClient reportingClient)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
        _reportingClient = reportingClient;
    }

    public async Task OnGetAsync(
        CancellationToken cancellationToken)
    {
        var user = HttpContext.GetFamilyHubsUser();

        long? organisationId = long.Parse(user.OrganisationId);
        var organisation = await _serviceDirectoryClient.GetOrganisationById(organisationId.Value, cancellationToken);

        OrgName = organisation.Name;

        var services = await _serviceDirectoryClient.GetServiceSummaries(organisationId.Value, pageSize: 1, cancellationToken: cancellationToken);
        var searches = await _reportingClient.GetServicesSearchesTotal(organisationId.Value, cancellationToken);
        var searchesPast7Days = await _reportingClient.GetServicesSearchesPast7Days(organisationId.Value, cancellationToken);

        Totals = new Dictionary<PerformanceDataType, long>()
        {
            { PerformanceDataType.LocalServices, services.TotalCount },
            { PerformanceDataType.SearchesTotal, searches },
            { PerformanceDataType.SearchesLast7Days, searchesPast7Days }
        };

        //var breakdown = await _reportingClient.GetServicesSearches4WeekBreakdown(organisationId.Value, cancellationToken);
        Breakdown = new Dictionary<DateTime, int>()
        {
            { DateTime.Today, 19 },
            { DateTime.Today.Subtract(TimeSpan.FromDays(7)), 119 },
            { DateTime.Today.Subtract(TimeSpan.FromDays(14)), 297 },
            { DateTime.Today.Subtract(TimeSpan.FromDays(21)), 178 }
        };
    }

    public string DateToWeekString(DateTime date)
    {
        var offsetDays = (int)date.DayOfWeek;
        var start = date.Subtract(TimeSpan.FromDays(offsetDays));
        var end = date.Add(TimeSpan.FromDays(6 - offsetDays));

        return $"{start:d MMMM} to {end:d MMMM}";
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
