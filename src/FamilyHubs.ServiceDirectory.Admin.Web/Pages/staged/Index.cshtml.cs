using System.Web;
using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Models;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Razor.Dashboard;
using FamilyHubs.SharedKernel.Razor.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.staged;

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
            yield return new Cell($"<a href=\"/manage-services/start-edit-service?serviceId={Item.Id}\">View</a>");
        }
    }
}

[Authorize(Roles = RoleGroups.AdminRole)]
public class IndexModel : HeaderPageModel, IDashboard<RowData>
{
    public const string PagePath = "/manage-services";

    public string? Title { get; set; }
    public string? OrganisationTypeContent { get; set; }
    public bool FilterApplied { get; set; }
    public string? CurrentServiceNameSearch { get; set; }
    public ServiceTypeArg? ServiceType { get; set; }

    private enum Column
    {
        Services,
        ActionLinks
    }

    private const int PageSize = 10;
    private static ColumnImmutable[] _columnImmutables =
    {
        new("Services", Column.Services.ToString()),
        new("<span class=\"govuk-visually-hidden\">Actions</span>", ColumnType: ColumnType.AlignedRight)
    };

    private IEnumerable<IColumnHeader> _columnHeaders = Enumerable.Empty<IColumnHeader>();
    private IEnumerable<IRow<RowData>> _rows = Enumerable.Empty<IRow<RowData>>();

    IEnumerable<IColumnHeader> IDashboard<RowData>.ColumnHeaders => _columnHeaders;
    public IEnumerable<IRow<RowData>> Rows => _rows;

    string? IDashboard<RowData>.TableClass => "app-services-dash";

    public IPagination Pagination { get; set; } = ILinkPagination.DontShow;
    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public IndexModel(IServiceDirectoryClient serviceDirectoryClient)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    public async Task OnGetAsync(
        CancellationToken cancellationToken,
        string? serviceType,
        string? columnName,
        SortOrder sort,
        int currentPage = 1,
        string? serviceNameSearch = null)
    {
        if (columnName == null || !Enum.TryParse(columnName, true, out Column column))
        {
            // default when first load the page, or user has manually changed the url
            column = Column.Services;
            sort = SortOrder.ascending;
        }

        FilterApplied = serviceNameSearch != null;

        ServiceType = GetServiceTypeArg(serviceType);

        var user = HttpContext.GetFamilyHubsUser();

        long? organisationId;
        switch (user.Role)
        {
            case RoleTypes.DfeAdmin:
                Title = "Services";
                organisationId = null;
                OrganisationTypeContent = " for Local Authorities and VCS organisations";
                break;

            case RoleTypes.LaManager or RoleTypes.LaDualRole or RoleTypes.VcsManager or RoleTypes.VcsDualRole:
                organisationId = long.Parse(user.OrganisationId);

                // don't assume that user has come through the welcome page by expecting the org in the cache
                var organisation = await _serviceDirectoryClient.GetOrganisationById(organisationId.Value, cancellationToken);

                Title = $"{organisation.Name} services";

                if (user.Role is RoleTypes.LaManager or RoleTypes.LaDualRole)
                {
                    OrganisationTypeContent = " in your Local Authority";
                }
                else
                {
                    OrganisationTypeContent = " in your VCS organisation";
                }
                break;

            default:
                throw new InvalidOperationException($"Unknown role: {user.Role}");
        }

        //todo: PaginatedList is in many places, there should be only one
        var services = await _serviceDirectoryClient.GetServiceSummaries(
            organisationId, serviceNameSearch, currentPage, PageSize, sort, cancellationToken);

        //todo: don't add serviceType if null
        string filterQueryParams = $"serviceNameSearch={HttpUtility.UrlEncode(serviceNameSearch)}&serviceType={ServiceType}";

        //todo: have combined factory that creates columns and pagination? (there's quite a bit of commonality)
        _columnHeaders = new ColumnHeaderFactory(_columnImmutables, PagePath, column.ToString(), sort, filterQueryParams)
            .CreateAll();
        _rows = GetRows(services);

        Pagination = new LargeSetLinkPagination<Column>(PagePath, services.TotalPages, currentPage, column, sort, filterQueryParams);

        CurrentServiceNameSearch = serviceNameSearch;
    }

    public IActionResult OnPost(
        CancellationToken cancellationToken,
        string? serviceType,
        string? columnName,
        SortOrder sort,
        string? serviceNameSearch,
        bool? clearFilter)
    {
        if (clearFilter == true)
        {
            serviceNameSearch = null;
        }

        return RedirectToPage($"{PagePath}/Index", new
        {
            columnName,
            sort,
            serviceNameSearch,
            serviceType
        });
    }

    private ServiceTypeArg? GetServiceTypeArg(string? serviceType)
    {
        if (!Enum.TryParse<ServiceTypeArg>(serviceType, out var serviceTypeEnum))
        {
            // it's only really needed for the dfe admin, but we'll require it for consistency (and for when we allow LAs to add VCS services)
            //throw new InvalidOperationException("ServiceType must be passed as a query parameter");
            return null;
        }
        return serviceTypeEnum;
    }

    private IEnumerable<Row> GetRows(PaginatedList<ServiceNameDto> services)
    {
        return services.Items.Select(s => new Row(new RowData(s.Id, s.Name)));
    }
}
