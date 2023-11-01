using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Razor.Dashboard;
using FamilyHubs.SharedKernel.Razor.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serilog.Parsing;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.Manage;

public record RowData(int Id, string Name);

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
            yield return new Cell(Item.Name);
            yield return new Cell($"<a href=\"Manage/ServiceDetail?id={Item.Id}\">View</a>");
        }
    }
}

//todo: manage/services or services/manage?
//todo: area?
[Authorize]
public class ServicesModel : PageModel, IDashboard<RowData>
{
    public string? Title { get; set; }

    private enum Column
    {
        Services,
        ActionLinks
    }

    private static ColumnImmutable[] _columnImmutables =
    {
        new("Services", Column.Services.ToString()),
        new("", Align: Align.Right)
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
            column = Column.Services;
            sort = SortOrder.ascending;
        }

        _columnHeaders = new ColumnHeaderFactory(_columnImmutables, "/Manage/Services", column.ToString(), sort)
            .CreateAll();
        _rows = GetSortedRows(column, sort);

        //Pagination = new LargeSetLinkPagination<Column>("/Manage/Services", searchResults.TotalPages, currentPage.Value, column, sort);
        Pagination = new LargeSetLinkPagination<Column>("/Manage/Services", 1, 1, column, sort);

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
            Column.Services => r.Item.Name,
            _ => throw new InvalidOperationException($"Unknown column: {column}")
        };
    }

    private Row[] GetExampleData()
    {
        return new Row[]
        {
            new(new RowData(1, "Child support")),
            new(new RowData(2, "Dorkings help")),
            new(new RowData(3, "Tummy time")),
            new(new RowData(4, "Mummy time"))
        };
    }
}