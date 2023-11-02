using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Razor.Dashboard;
using FamilyHubs.SharedKernel.Razor.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serilog.Parsing;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.Locations.Pages;

public record RowData(int Id, string Location, string LocationType);

public class Row : IRow<RowData>
{
    public RowData Item { get; }

    public Row(RowData data)
    {
        Item = data;
    }

    public IEnumerable<ICell> Cells
    {
        get
        {
            yield return new Cell(Item.Location);
            yield return new Cell(Item.LocationType) ;
            yield return new Cell($"<a href=\"/locations/LocationDetail?id={Item.Id}\">View</a>");
        }
    }
}

[Authorize]
public class ManageLocationsModel : PageModel, IDashboard<RowData>
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
    private IEnumerable<IRow<RowData>> _rows = Enumerable.Empty<IRow<RowData>>();

    IEnumerable<IColumnHeader> IDashboard<RowData>.ColumnHeaders => _columnHeaders;
    IEnumerable<IRow<RowData>> IDashboard<RowData>.Rows => _rows;

    public void OnGet(string? columnName, SortOrder sort)
    {
        if (columnName == null || !Enum.TryParse(columnName, true, out Column column))
        {
            // default when first load the page, or user has manually changed the url
            column = Column.Location;
            sort = SortOrder.ascending;
        }

        _columnHeaders = new ColumnHeaderFactory(_columnImmutables, "/Locations/ManageLocations", column.ToString(), sort)
            .CreateAll();
        _rows = GetSortedRows(column, sort);

        //Pagination = new LargeSetLinkPagination<Column>("/Locations/ManageLocations", searchResults.TotalPages, currentPage.Value, column, sort);
        Pagination = new LargeSetLinkPagination<Column>("/Locations/ManageLocations", 1, 1, column, sort);

        var user = HttpContext.GetFamilyHubsUser();
        Title = user.Role switch
        {
            RoleTypes.DfeAdmin => "Services",
            RoleTypes.LaManager or RoleTypes.LaDualRole => "[Local authority] services",
            RoleTypes.VcsManager or RoleTypes.VcsDualRole => "[VCS organisation] services",
            _ => throw new InvalidOperationException($"Unknown role: {user.Role}")
        };
    }

    string? IDashboard<RowData>.TableClass => "app-services-dash";

    public IPagination Pagination { get; set; } = ILinkPagination.DontShow;

    private IEnumerable<Row> GetSortedRows(Column column, SortOrder sort)
    {
        if (sort == SortOrder.ascending)
        {
            return GetExampleData().OrderBy(r => GetValue(column, r));
        }

        return GetExampleData().OrderByDescending(r => GetValue(column, r));
    }

    private static string GetValue(Column column, Row r)
    {
        return column switch
        {
            Column.Location => r.Item.Location,
            Column.LocationType => r.Item.LocationType,
            _ => throw new InvalidOperationException($"Unknown column: {column}")
        };
    }

    private Row[] GetExampleData()
    {
        return new Row[]
        {
            new(new RowData(1, "Southmead Children's Centre, Doncaster Road, Southmead, Bristol, BS10 5PW", "test")),
            new(new RowData(2, "Welfare Rights and Money Advice Service (100TS), Bristol City Council, PO Box 3399, Bristol, BS1 9NE", "")),
            new(new RowData(3, "MusicSpace, BS3 Community Centre, Beauley Road, Bristol, BS3 1QG", "" )),
            new(new RowData(4, "Happy Maps, 34 Longfield Road, Bristol, BS7 9AG", ""))
        };
    }
}