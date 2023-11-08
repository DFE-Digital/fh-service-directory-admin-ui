using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Models;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Razor.Dashboard;
using FamilyHubs.SharedKernel.Razor.Pagination;
using Microsoft.AspNetCore.Authorization;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.Manage;

public record RowData(long Id, string Name);

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
public class ServicesModel : HeaderPageModel, IDashboard<RowData>
{
    public string? Title { get; set; }

    private enum Column
    {
        Services,
        ActionLinks
    }

    private const int PageSize = 10;
    private static ColumnImmutable[] _columnImmutables =
    {
        new("Services", Column.Services.ToString()),
        new("", Align: Align.Right)
    };

    private IEnumerable<IColumnHeader> _columnHeaders = Enumerable.Empty<IColumnHeader>();
    private IEnumerable<IRow<RowData>> _rows = Enumerable.Empty<IRow<RowData>>();

    IEnumerable<IColumnHeader> IDashboard<RowData>.ColumnHeaders => _columnHeaders;
    IEnumerable<IRow<RowData>> IDashboard<RowData>.Rows => _rows;

    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public ServicesModel(
        IServiceDirectoryClient serviceDirectoryClient)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    //todo: currentpage as int
    public async Task OnGet(string? columnName, SortOrder sort, int? currentPage = 1)
    {
        if (columnName == null || !Enum.TryParse(columnName, true, out Column column))
        {
            // default when first load the page, or user has manually changed the url
            column = Column.Services;
            sort = SortOrder.ascending;
        }

        var user = HttpContext.GetFamilyHubsUser();

        long? organisationId;
        switch (user.Role)
        {
            case RoleTypes.DfeAdmin:
                Title = "Services";
                organisationId = null;
                break;
            case RoleTypes.LaManager or RoleTypes.LaDualRole or RoleTypes.VcsManager or RoleTypes.VcsDualRole:
                organisationId = long.Parse(user.OrganisationId);
                //todo: don't assume that user has come through the welcome page, they might have bookmarked this page
                //var organisation = await _cacheService.RetrieveOrganisationWithService();
                //todo: handle null (by fetching org from api). if getting nothing else from the cache, could just go straight to the api, if we add a new slim endpoint
                var organisation = await _serviceDirectoryClient.GetOrganisationById(organisationId.Value);
                //^^ gets services too, but need the services paginated

                Title = $"{organisation!.Name} services";
                break;
            //case RoleTypes.VcsManager or RoleTypes.VcsDualRole:
            //    Title = $"{organisation!.Name} services";
            //    services = new List<ServiceDto>();
            //    break;
            default:
                throw new InvalidOperationException($"Unknown role: {user.Role}");
        }

        //todo: PaginatedList is in many places, there can be only one
        var services = await _serviceDirectoryClient.GetServiceSummaries(
            organisationId, currentPage!.Value, PageSize, sort);

        _columnHeaders = new ColumnHeaderFactory(_columnImmutables, "/Manage/Services", column.ToString(), sort)
            .CreateAll();
        _rows = GetRows(services);

        Pagination = new LargeSetLinkPagination<Column>("/Manage/Services", services.TotalPages, currentPage!.Value, column, sort);
    }

    string? IDashboard<RowData>.TableClass => "app-services-dash";

    public IPagination Pagination { get; set; } = ILinkPagination.DontShow;

    private IEnumerable<Row> GetRows(PaginatedList<ServiceNameDto> services)
    {
        return services.Items.Select(s => new Row(new RowData(s.Id, s.Name)));
    }
}