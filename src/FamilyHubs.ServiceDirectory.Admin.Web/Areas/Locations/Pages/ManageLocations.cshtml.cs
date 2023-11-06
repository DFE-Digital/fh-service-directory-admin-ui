using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Razor.Dashboard;
using FamilyHubs.SharedKernel.Razor.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

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
            yield return new Cell(GetLocationDescryption(Item));
            yield return new Cell(GetLocationType(Item));
            yield return new Cell($"<a href=\"/locations/LocationDetail?id={Item.Id}\">View</a>");
        }
    }

    private string GetLocationDescryption(LocationDto location)
    {
        var parts = new string[] { Item.Name, Item.Address1, Item.Address2 ?? "", Item.City, Item.PostCode };
        return string.Join(", ", parts.Where(p=> !string.IsNullOrEmpty(p)));
    }

    private string GetLocationType(LocationDto location)
    {
        return location.LocationType == LocationType.FamilyHub ? "FAMILY HUB" : ""; 
    }
}

[Authorize]
public class ManageLocationsModel : PageModel, IDashboard<LocationDto>
{
    public string? Title { get; set; }

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
    string? IDashboard<LocationDto>.TableClass => "app-services-dash";

    public IPagination Pagination { get; set; } = ILinkPagination.DontShow;


    public ManageLocationsModel(IServiceDirectoryClient serviceDirectoryClient)
    {
        _serviceDirectoryClient = serviceDirectoryClient;    
    }

    public async Task OnGet(string? columnName, SortOrder sort, int? currentPage = 1)
    {
        if (columnName == null || !Enum.TryParse(columnName, true, out Column column))
        {
            // default when first load the page, or user has manually changed the url
            column = Column.Location;
            sort = SortOrder.ascending;
        }

        _columnHeaders = new ColumnHeaderFactory(_columnImmutables, "/Locations/ManageLocations", column.ToString(), sort).CreateAll();
        var locations = await _serviceDirectoryClient.GetLocations(sort == SortOrder.ascending, column.ToString(), currentPage!.Value);
        _rows = locations.Items.Select(r => new LocationDashboardRow(r));        

        Pagination = new LargeSetLinkPagination<Column>("/Locations/ManageLocations", locations.TotalPages, currentPage!.Value, column, sort);        

        var user = HttpContext.GetFamilyHubsUser();
        Title = user.Role switch
        {
            RoleTypes.DfeAdmin => "Services",
            RoleTypes.LaManager or RoleTypes.LaDualRole => "[Local authority] services",
            RoleTypes.VcsManager or RoleTypes.VcsDualRole => "[VCS organisation] services",
            _ => throw new InvalidOperationException($"Unknown role: {user.Role}")
        };
    }

  
   
}