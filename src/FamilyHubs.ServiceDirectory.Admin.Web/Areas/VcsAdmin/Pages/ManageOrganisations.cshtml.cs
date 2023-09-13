using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Models;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Razor.Pagination;
using Microsoft.AspNetCore.Mvc;
using static FamilyHubs.ServiceDirectory.Admin.Web.Components.SortHeaderComponent;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.VcsAdmin.Pages
{
    public class ManageOrganisationsModel : HeaderPageModel
    {
        private const int _pageSize = 10;
        private readonly IServiceDirectoryClient _serviceDirectoryClient;
        private readonly ICacheService _cacheService;

        public string OrganisationColumn { get; } = "Organisation";
        public string LaColumn { get; } = "LocalAuthority";
        public bool IsDfeAdmin { get; set; } = false;
        public IPagination Pagination { get; set; }
        public PaginatedList<OrganisationModel> PaginatedOrganisations { get; set; }

        [BindProperty]
        public int PageNum { get; set; } = 1; //Do not change variable name, this is what is posted by the pagination partial

        [BindProperty]
        public string SortBy { get; set; } = string.Empty;

        public ManageOrganisationsModel(IServiceDirectoryClient serviceDirectoryClient, ICacheService cacheService)
        {
            _serviceDirectoryClient = serviceDirectoryClient;
            _cacheService = cacheService;
            PaginatedOrganisations = new PaginatedList<OrganisationModel>();
            Pagination = new DontShowPagination();
        }

        public async Task OnGet(int? pageNumber, string? sortBy)
        {
            IsDfeAdmin = HttpContext.IsUserDfeAdmin();

            if (pageNumber.HasValue)
                PageNum = pageNumber.Value;

            if (!string.IsNullOrEmpty(sortBy))
                SortBy = sortBy;

            await SetPaginatedList();
            await CacheParametersToBackButton();
        }

        public IActionResult OnPost()
        {
            var query = CreateQueryParameters();
            return RedirectToPage(query);
        }

        public string GetTestId(string name)
        {
            return name.Replace(" ", "");
        }

        private async Task SetPaginatedList()
        {
            var vcsOrganisations = await GetPermittedOrganisations();
            vcsOrganisations = Sort(vcsOrganisations);

            PaginatedOrganisations = new PaginatedList<OrganisationModel>(
                vcsOrganisations.Skip((PageNum - 1) * _pageSize).Take(_pageSize).ToList(),
                vcsOrganisations.Count(),
                PageNum,
                _pageSize);

            if(PaginatedOrganisations.TotalCount> 0)
            {
                Pagination = new LargeSetPagination(PaginatedOrganisations.TotalPages, PageNum);
            }

        }

        /// <summary>
        /// Gets the VCS organisations the user is permitted to see
        /// </summary>
        private async Task<IEnumerable<OrganisationModel>> GetPermittedOrganisations()
        {
            var user = HttpContext.GetFamilyHubsUser();
            List<OrganisationDto> organisations;

            if (user.Role == RoleTypes.DfeAdmin)
            {
                organisations = await _serviceDirectoryClient.GetOrganisations();
            }
            else
            {
                organisations = await _serviceDirectoryClient.GetOrganisationByAssociatedOrganisation(long.Parse(user.OrganisationId));
            }

            var vcsOrganisations = organisations
                .Where(x => x.OrganisationType == Shared.Enums.OrganisationType.VCFS)
                .Select(org => new OrganisationModel
                {
                    OrganisationId = org.Id,
                    OrganisationName = org.Name,
                    LocalAuthority = organisations.Find(x => x.Id == org.AssociatedOrganisationId)?.Name ?? string.Empty
                });

            return vcsOrganisations;
        }

        private IEnumerable<OrganisationModel> Sort(IEnumerable<OrganisationModel> organisations)
        {
            if (string.IsNullOrEmpty(SortBy))
                return organisations;

            if(SortBy == $"{OrganisationColumn}_{SortOrder.Ascending.ToString()}")
                return organisations.OrderBy(x => x.OrganisationName);

            if (SortBy == $"{OrganisationColumn}_{SortOrder.Descending.ToString()}")
                return organisations.OrderByDescending(x => x.OrganisationName);

            if (SortBy == $"{LaColumn}_{SortOrder.Ascending.ToString()}")
                return organisations.OrderBy(x => x.LocalAuthority);

            if (SortBy == $"{LaColumn}_{SortOrder.Descending.ToString()}")
                return organisations.OrderByDescending(x => x.LocalAuthority);

            throw new Exception("SortBy not recognised");
        }

        private object CreateQueryParameters()
        {
            var routeValues = new Dictionary<string, object>();

            routeValues.Add("pageNumber", PageNum);
            routeValues.Add("sortBy", SortBy);

            return routeValues;
        }

        /// <summary>
        /// If someone goes to the edit page then clicks the back button, we want them to return to the
        /// paginated page they where on. This stores the link to get them back to the current page
        /// </summary>
        private async Task CacheParametersToBackButton()
        {
            var queryDictionary = (Dictionary<string, object>)CreateQueryParameters();
            var backButtonPath = "/VcsAdmin/ManageOrganisations?";

            foreach (var parameter in queryDictionary)
            {
                backButtonPath += $"{parameter.Key}={parameter.Value}&";
            }

            backButtonPath = backButtonPath.Remove(backButtonPath.Length - 1, 1);//Remove unwanted '&' or '?'

            await _cacheService.StoreCurrentPageName(backButtonPath);
        }

        public class OrganisationModel
        {
            public long OrganisationId { get; set; }
            public string OrganisationName { get; set; } = string.Empty;
            public string LocalAuthority { get; set; } = string.Empty;
        }
    }
}
