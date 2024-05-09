using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.performance_data;

[Authorize(Roles=RoleGroups.LaManagerOrDualRole)]
public class FindPerformanceDataModel : HeaderPageModel
{
    public const string PagePath = "/performance-data";

    public string Title => "Performance data for Find support for your family";
    public string OrgName { get; private set; }

    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public FindPerformanceDataModel(IServiceDirectoryClient serviceDirectoryClient)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    public async Task OnGetAsync(
        CancellationToken cancellationToken)
    {
        var user = HttpContext.GetFamilyHubsUser();

        long? organisationId = long.Parse(user.OrganisationId);
        var organisation = await _serviceDirectoryClient.GetOrganisationById(organisationId.Value, cancellationToken);

        OrgName = organisation.Name;
    }

    public Dictionary<PerformanceDataType, int> GetTotals()
    {
        return new Dictionary<PerformanceDataType, int>()
        {
            { PerformanceDataType.LocalServices, 4983 },
            { PerformanceDataType.SearchesLastDay, 4232 },
            { PerformanceDataType.SearchesLast7Days, 23 }
        };
    }

    public Dictionary<DateTime, int> GetRecentSearches()
    {
        return new Dictionary<DateTime, int>()
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
    public static readonly PerformanceDataType SearchesLastDay = new("Searches");
    public static readonly PerformanceDataType SearchesLast7Days = new("Searches in the last 7 days");
}
