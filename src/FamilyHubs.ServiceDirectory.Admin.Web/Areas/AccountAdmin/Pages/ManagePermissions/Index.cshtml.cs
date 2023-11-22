using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Models;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Razor.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Web;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages.ManagePermissions
{
    [Authorize(Roles = $"{RoleTypes.DfeAdmin},{RoleTypes.LaDualRole},{RoleTypes.LaManager}")]
    public class IndexModel : HeaderPageModel
    {
        private readonly IIdamClient _idamClient;
        private readonly ICacheService _cacheService;
        public IPagination Pagination { get; set; }
        public PaginatedList<AccountDto> PaginatedList { get; set; }

        [BindProperty]
        public int PageNum { get; set; } = 1;

        [BindProperty]
        public string Name { get; set; } = string.Empty;

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Organisation { get; set; } = string.Empty;

        [BindProperty]
        public bool IsLaUser { get; set; }

        [BindProperty]
        public bool IsVcsUser { get; set; }

        [BindProperty]
        public string SortBy { get; set; } = string.Empty;

        public IndexModel(IIdamClient idamClient, ICacheService cacheService)
        {
            _idamClient = idamClient;
            PaginatedList = new PaginatedList<AccountDto>();
            Pagination = new DontShowPagination();
            _cacheService = cacheService;
        }

        public async Task OnGet(int? pageNumber, string? name, string? email, string? organisation, bool? isLa, bool? isVcs, string? sortBy)
        {
            ResolveQueryParameters(pageNumber, name, email, organisation, isLa, isVcs, sortBy);
            await CacheParametersToBackButton();
            var users = await _idamClient.GetAccounts(PageNum, name, email, organisation, isLa, isVcs, sortBy);

            if (users != null)
            {
                PaginatedList = users;
                Pagination = new LargeSetPagination(users.TotalPages, PageNum);
            }
        }

        public IActionResult OnPost()
        {            
            var query = CreateQueryParameters();
            return RedirectToPage(query);
        }

        public IActionResult OnClearFilters()
        {
            return RedirectToPage("/Index");
        }

        private void ResolveQueryParameters(int? pageNumber, string? name, string? email, string? organisation, bool? isLaUser, bool? isVcsUser, string? sortBy)
        {
            if (pageNumber != null) PageNum = pageNumber.Value;
            if (name != null) Name = HttpUtility.UrlEncode(name);
            if (email != null) Email = HttpUtility.UrlEncode(email); 
            if (organisation != null) Organisation = organisation;
            if (isLaUser != null) IsLaUser = isLaUser.Value;
            if (isVcsUser != null) IsVcsUser = isVcsUser.Value;
            if (sortBy != null) SortBy = sortBy;
        }

        private object CreateQueryParameters()
        {
 
            var routeValues = new Dictionary<string, object>();

            routeValues.Add("pageNumber", PageNum);
            if (Name != null) routeValues.Add("name", Name);
            if (Email != null) routeValues.Add("email", Email);
            if (Organisation != null) routeValues.Add("organisation", Organisation);
            routeValues.Add("isLa", IsLaUser);
            routeValues.Add("isVcs", IsVcsUser);
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
            StringBuilder backButtonPath = new StringBuilder();
            backButtonPath.Append("/AccountAdmin/ManagePermissions?");

            foreach(var parameter in queryDictionary)
            {
                backButtonPath.Append($"{parameter.Key}={parameter.Value}&");
            }

            backButtonPath = backButtonPath.Remove(backButtonPath.Length - 1, 1);//Remove unwanted '&' or '?'

            await _cacheService.StoreCurrentPageName(backButtonPath.ToString());
        }

        public static string OrganisationName(AccountDto account)
        {
            var organisationName = account?.Claims?.Find(x => x.Name == "OrganisationName")?.Value;
            return organisationName ?? string.Empty;
        }
    }
}
