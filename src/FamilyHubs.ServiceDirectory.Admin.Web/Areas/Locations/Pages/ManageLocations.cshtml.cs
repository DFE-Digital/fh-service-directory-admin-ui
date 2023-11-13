using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Razor.Dashboard;
using FamilyHubs.SharedKernel.Razor.Pagination;
using Microsoft.AspNetCore.Mvc;
using System.Web;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.Locations.Pages;

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
            yield return new Cell(GetLocationType(Item));
            yield return new Cell($"<a href=\"\">View</a>");
        }
    }

    private string GetLocationDescription(LocationDto location)
    {
        var parts = new string[] { Item.Name, Item.Address1, Item.Address2 ?? "", Item.City, Item.PostCode };
        return string.Join(", ", parts.Where(p => !string.IsNullOrEmpty(p)));
    }

    private string GetLocationType(LocationDto location)
    {
        return location.LocationType == LocationType.FamilyHub
            ? $"<div class=\"govuk-tag\">FAMILY HUB</div>"
            : "";
    }
}

public class ManageLocationsModel : HeaderPageModel, IDashboard<LocationDto>
{
    public string? Title { get; set; }
    public string? SubTitle { get; set; }
    public int ResultCount { get; set; }

    [BindProperty]
    public string SearchName { get; set; } = string.Empty;

    [BindProperty]
    public bool IsFamilyHub { get; set; } = false;

    [BindProperty]
    public bool IsNonFamilyHub { get; set; } = false;

    public bool IsVcsUser { get; set; } = false;

    private enum Column
    {
        Location,
        LocationType,
        ActionLinks
    }

    private static ColumnImmutable[] _columnImmutables =
    {
        new("Location", Column.Location.ToString()),
        new("Location Type", Column.LocationType.ToString()),
        new("")
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

        string filterQueryParams = $"searchName={HttpUtility.UrlEncode(searchName)}&isFamilyHubParam={isFamilyHubParam}&isNonFamilyHubParam={isNonFamilyHubParam}";
        SearchName = searchName ?? string.Empty;
        IsFamilyHub = isFamilyHubParam ?? false;
        IsNonFamilyHub = isNonFamilyHubParam ?? false;

        _columnHeaders = new ColumnHeaderFactory(_columnImmutables, "/Locations/ManageLocations", column.ToString(), sort).CreateAll();

        var locations = new Shared.Models.PaginatedList<LocationDto>();
        if (user.Role == RoleTypes.DfeAdmin)
        {
            locations = await _serviceDirectoryClient.GetLocations(sort == SortOrder.ascending, column.ToString(), searchName, IsFamilyHub, IsNonFamilyHub, currentPage!.Value);
        }
        else
        {
            long organisationId = HttpContext.GetUserOrganisationId();
            locations = await _serviceDirectoryClient.GetLocationsByOrganisationId(organisationId, sort == SortOrder.ascending, column.ToString(), searchName, IsFamilyHub, IsNonFamilyHub, currentPage!.Value);
        }

        _rows = locations.Items.Select(r => new LocationDashboardRow(r));
        ResultCount = locations.Items.Count();

        Pagination = new LargeSetLinkPagination<Column>("/Locations/ManageLocations", locations.TotalPages, currentPage!.Value, column, sort, filterQueryParams);


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
            RoleTypes.DfeAdmin => "View existing locations",
            RoleTypes.LaManager or RoleTypes.LaDualRole => "View existing locations in your local authority and add locations",
            RoleTypes.VcsManager or RoleTypes.VcsDualRole => "View existing locations in your organisation",
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
        return RedirectToPage("/locations/managelocations");
    }

    private object CreateQueryParameters()
    {

        var routeValues = new Dictionary<string, object>();

        if (SearchName != null) routeValues.Add("searchName", SearchName);
        routeValues.Add("isFamilyHubParam", IsFamilyHub);
        routeValues.Add("isNonFamilyHubParam", IsNonFamilyHub);

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