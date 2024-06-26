using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Models;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Razor.Dashboard;
using FamilyHubs.SharedKernel.Razor.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Web;
using FamilyHubs.ServiceDirectory.Shared.Display;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_locations;

public class LocationDashboardRow : IRow<LocationDto>
{
    public LocationDto Item { get; }

    public LocationDashboardRow(LocationDto data)
    {
        Item = data;
    }

    public IEnumerable<ICell> Cells
    {
        get
        {
            yield return new Cell(GetLocationDescription(Item));
            yield return new Cell($"<a href=\"/manage-locations/start-edit-location?locationId={Item.Id}\">View details</a>");
            //yield return new Cell($"<a href=\"/manage-locations/services-at-location?locationId={Item.Id}\">View services</a>");
        }
    }

    private string GetLocationDescription(LocationDto location)
    {
        return string.Join(", ", location.GetAddress());
    }
}

[Authorize(Roles = RoleGroups.AdminRole)]
public class ManageLocationsModel : HeaderPageModel, IDashboard<LocationDto>
{
    public const string PagePath = "/manage-locations";
    public string? Title { get; set; }
    public string? SubTitle { get; set; }
    public int ResultCount { get; set; }

    [BindProperty]
    public string SearchName { get; set; } = string.Empty;

    [BindProperty]
    public bool IsFamilyHub { get; set; }

    public bool IsVcsUser { get; set; }

    private enum Column
    {
        Location,
        LocationType,
        ActionLinks
    }

    private static ColumnImmutable[] _columnImmutables =
    {
        new("Location", Column.Location.ToString()),
        new(""),
        //new("")
    };

    private IEnumerable<IColumnHeader> _columnHeaders = Enumerable.Empty<IColumnHeader>();
    private IEnumerable<IRow<LocationDto>> _rows = Enumerable.Empty<IRow<LocationDto>>();
    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    IEnumerable<IColumnHeader> IDashboard<LocationDto>.ColumnHeaders => _columnHeaders;
    IEnumerable<IRow<LocationDto>> IDashboard<LocationDto>.Rows => _rows;
    string? IDashboard<LocationDto>.TableClass => "app-locations-dash";

    public IPagination Pagination { get; set; } = ILinkPagination.DontShow;

    public ManageLocationsModel(IServiceDirectoryClient serviceDirectoryClient)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    public async Task OnGet(string? columnName, SortOrder sort, int? currentPage = 1, string? searchName = "", bool? isFamilyHubParam = false, bool? isNonFamilyHubParam = false)
    {
        var user = HttpContext.GetFamilyHubsUser();
        if (columnName == null || !Enum.TryParse(columnName, true, out Column column))
        {
            // default when first load the page, or user has manually changed the url
            column = Column.Location;
            sort = SortOrder.ascending;
        }
        IsVcsUser = user.Role == RoleTypes.VcsManager || user.Role == RoleTypes.VcsDualRole;

        string filterQueryParams = $"searchName={HttpUtility.UrlEncode(searchName)}&isFamilyHubParam={isFamilyHubParam}";
        SearchName = searchName ?? string.Empty;
        IsFamilyHub = isFamilyHubParam ?? false;

        _columnHeaders = new ColumnHeaderFactory(_columnImmutables, PagePath, column.ToString(), sort, filterQueryParams).CreateAll();

        PaginatedList<LocationDto> locations;
        if (user.Role == RoleTypes.DfeAdmin)
        {
            locations = await _serviceDirectoryClient.GetLocations(sort == SortOrder.ascending, column.ToString(), searchName, IsFamilyHub, currentPage!.Value);
        }
        else
        {
            long organisationId = HttpContext.GetUserOrganisationId();
            locations = await _serviceDirectoryClient.GetLocationsByOrganisationId(organisationId, sort == SortOrder.ascending, column.ToString(), searchName, IsFamilyHub, currentPage!.Value);
        }

        _rows = locations.Items.Select(r => new LocationDashboardRow(r));
        ResultCount = locations.Items.Count;

        Pagination = new LargeSetLinkPagination<Column>(PagePath, locations.TotalPages, currentPage!.Value, column, sort, filterQueryParams);

        var organisationName = await GetOrganisationName(HttpContext.GetUserOrganisationId());
        Title = user.Role switch
        {
            RoleTypes.DfeAdmin => "Locations",
            RoleTypes.LaManager or RoleTypes.LaDualRole => $"{organisationName} Locations",
            RoleTypes.VcsManager or RoleTypes.VcsDualRole => $"{organisationName} Locations",
            _ => throw new InvalidOperationException($"Unknown role: {user.Role}")
        };
        SubTitle = user.Role switch
        {
            RoleTypes.DfeAdmin => "View and edit existing locations",
            RoleTypes.LaManager or RoleTypes.LaDualRole => "View and edit existing locations in your local authority",
            RoleTypes.VcsManager or RoleTypes.VcsDualRole => "View and edit existing locations in your organisation",
            _ => throw new InvalidOperationException($"Unknown role: {user.Role}")
        };
    }

    public IActionResult OnPost()
    {
        var query = CreateQueryParameters();
        return RedirectToPage(query);
    }

    public IActionResult OnClearFilters()
    {
        return RedirectToPage($"{PagePath}/Index");
    }

    private object CreateQueryParameters()
    {

        var routeValues = new Dictionary<string, object>();

        if (SearchName != null) routeValues.Add("searchName", SearchName);
        routeValues.Add("isFamilyHubParam", IsFamilyHub);

        return routeValues;
    }

    private async Task<string> GetOrganisationName(long organisationId)
    {
        if (organisationId > 0)
        {
            var organisation = await _serviceDirectoryClient.GetOrganisationById(organisationId);
            if (organisation is not null)
            {
                return organisation.Name;
            }
        }

        return "";
    }
}