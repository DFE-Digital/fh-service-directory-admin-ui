using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Models;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Razor.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static FamilyHubs.ServiceDirectory.Admin.Web.Components.SortHeaderComponent;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.VcsAdmin.Pages
{
    public class ManageOrganisationsModel : PageModel
    {
        private const int _pageSize = 10;
        public string OrganisationColumn { get; } = "Organisation";
        public string LaColumn { get; } = "LocalAuthority";

        private readonly IServiceDirectoryClient _serviceDirectoryClient;

        public IPagination Pagination { get; set; }
        public PaginatedList<OrganisationModel> PaginatedOrganisations { get; set; }

        [BindProperty]
        public int PageNum { get; set; } = 1; //Do not change variable name, this is what is posted by the pagination partial

        [BindProperty]
        public string SortBy { get; set; } = string.Empty;

        public ManageOrganisationsModel(IServiceDirectoryClient serviceDirectoryClient)
        {
            _serviceDirectoryClient = serviceDirectoryClient;
            PaginatedOrganisations = new PaginatedList<OrganisationModel>();
            Pagination = new DontShowPagination();
        }

        public async Task OnGet(int? pageNumber, string? sortBy)
        {
            if (pageNumber.HasValue)
                PageNum = pageNumber.Value;

            if (!string.IsNullOrEmpty(sortBy))
                SortBy = sortBy;

            await SetPaginatedList();
        }

        public IActionResult OnPost()
        {
            var query = CreateQueryParameters();
            return RedirectToPage(query);
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
                    LocalAuthority = organisations.Where(x => x.Id == org.AssociatedOrganisationId).FirstOrDefault()?.Name ?? string.Empty
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

        public class OrganisationModel
        {
            public long OrganisationId { get; set; }
            public string OrganisationName { get; set; } = string.Empty;
            public string LocalAuthority { get; set; } = string.Empty;
        }
    }
}
