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
using static System.Net.WebRequestMethods;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.staged;

/*
 * system prompt:
 *
   review the user content for suitability to be shown an a GOV.UK public site.
   reply with a json object only - do not add any pre or post amble.
   the json object should be in the following format:
   {
   "ReadingLevel": 9,
   "InappropriateLanguage": {
     "Flag": true,
     "Instances": [
       { 
         "Reason": "Contains swear words",
         "Content": "bloody stupid idiots",
       },
       { 
         "Reason": "Inappropriate slang",
         "Content": "OMFG this is fun",
       }
   ]
   },
   "PoliticisedSentiment": {
     "Flag": true,
     "Instances": [
       { 
         "Reason": "Negative sentiment towards conservative party",
         "Content": "We help people the Tories couldn't care less about",
       }
   ]
   },
}
   
   The ReadingLevel integer should be the reading age required to read and comprehend the content. Consider sentence complexity, vocabulary, content depth, paragraph length, and topic relevance.
   
InappropriateLanguage should flag whether the content contains inappropriate language.
If the flag is true, then the Instances array should contain objects with a Reason property and a Content property.
The Reason property should be a string describing why the content is inappropriate, and the Content property should be the text that is inappropriate.

PoliticisedSentiment is similar to InappropriateLanguage, but should flag whether the content contains politicised sentiment.
 *
 *
 */

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
            yield return new Cell(GetPassCell(false));
            yield return new Cell(GetPassCell(null));
            yield return new Cell(GetPassCell(false));
            yield return new Cell(GetPassCell(true));
            yield return new Cell(GetPassCell(true));
            //todo: actions approve/edit/delete/rerun analysis (either as separate action or do after edit?)
            //todo: separate delete into with/without prejudice, with prejudice marks same service resubmitted as auto-rejected
            // without prejudice means will show up again in the list if re-ingested
            yield return new Cell($"<a href=\"/manage-services/start-edit-service?serviceId={Item.Id}\">Approve</a>");
        }
    }

    private string GetPassCell(bool? pass)
    {
        return pass switch
        {
            true => "<strong class=\"govuk-tag govuk-tag--green\">Pass</strong>",
            false => "<strong class=\"govuk-tag govuk-tag--red\">Fail</strong>",
            null => "<strong class=\"govuk-tag govuk-tag--orange\">Not run</strong>",
        };
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
        SecurityAnalysis,
        FindRender,
        ContentAnalysis,
        ReadingLevel,
        Locations,
        Approvals,
        ActionLinks
    }

    //todo: we could pick up services from
    // staging database
    // same database, different schema name
    // inactive (and not archived) from same db
    // have a new table to store meta-data about services/locations

    //todo: service is no good without approved locations - best way to handle?

    //todo: need way to approve all in batch

    private const int PageSize = 10;
    private static ColumnImmutable[] _columnImmutables =
    {
        //todo: either going to have to combine some of these, or have a global ok or not and a details page

        new("Services", Column.Services.ToString()),
        //todo: run security static analysis, or ask llm to check for potential security issues, such as sql injection, etc
        new("Security Analysis", Column.SecurityAnalysis.ToString()),
        // green tick icons or red cross icons with visually hidden text or just PASS/FAIL badge
        //todo: link to page that shows the results of the auto render tests
        // broken down into search result page/ details page/ anywhere else service details are rendered : dashboard?
        // could have iframes showing the rendered pages (or parts of)
        //todo: if security fails, then don't try and render the service
        new("Find Render", Column.FindRender.ToString()),
        //todo: link to page that shows the results of the auto analysis tests (pass to llm to check content for political bias (especially during elections), inappropriate language (e.g. spelling), PII, grammar,  
        new("Content Analysis", Column.ContentAnalysis.ToString()),
        //todo: GDS recommends reading level suitable for typical 9 year old. get llm to assign a reading age level
        new("Reading Level", Column.ReadingLevel.ToString()),
        // all approved? can't approve service until locations are approved
        new("Locations", Column.Locations.ToString()),
        //todo: check external links are reachable, and don't have inappropriate content
        new("External links", Column.ReadingLevel.ToString()),
        //todo: check style according to these pages...
        //https://www.gov.uk/guidance/style-guide/a-to-z-of-gov-uk-style
        //https://www.gov.uk/guidance/content-design/writing-for-gov-uk
        new("Style", Column.ReadingLevel.ToString()),
        //todo: multiple approvers? dfe/la/vcs (if vcs?)
        new("Approvals", Column.Approvals.ToString()),

        //todo: upload id/date?

        // actions would be approve/view (or get to that through render page?)/manage locations/archive
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
