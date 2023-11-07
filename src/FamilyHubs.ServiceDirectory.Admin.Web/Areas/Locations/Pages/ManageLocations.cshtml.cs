using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
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
            yield return new Cell(GetLocationDescription(Item));
            yield return new Cell(GetLocationType(Item));
            yield return new Cell($"<a href=\"/locations/LocationDetail?id={Item.Id}\">View</a>");
        }
    }

    private string GetLocationDescription(LocationDto location)
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
    public string? SubTitle { get; set; }

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
        var user = HttpContext.GetFamilyHubsUser();
        if (columnName == null || !Enum.TryParse(columnName, true, out Column column))
        {
            // default when first load the page, or user has manually changed the url
            column = Column.Location;
            sort = SortOrder.ascending;
        }

        _columnHeaders = new ColumnHeaderFactory(_columnImmutables, "/Locations/ManageLocations", column.ToString(), sort).CreateAll();

        var locations = new Shared.Models.PaginatedList<LocationDto>();
        if ( user.Role == RoleTypes.DfeAdmin )
        {
             locations = await _serviceDirectoryClient.GetLocations(sort == SortOrder.ascending, column.ToString(), currentPage!.Value);
        }
        else
        {
            long organisationId;
            long.TryParse(user.OrganisationId, out organisationId);
            locations = await _serviceDirectoryClient.GetLocationsByOrganisationId(organisationId, sort == SortOrder.ascending, column.ToString(), currentPage!.Value);
        }
        
        _rows = locations.Items.Select(r => new LocationDashboardRow(r));        

        Pagination = new LargeSetLinkPagination<Column>("/Locations/ManageLocations", locations.TotalPages, currentPage!.Value, column, sort);        

        
        var organisationName = await GetOrganisationName(user.OrganisationId);
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
    
    private async Task<string> GetOrganisationName(string organisationIdString)
    {
        long organisationId;
        if (long.TryParse(organisationIdString, out organisationId))
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