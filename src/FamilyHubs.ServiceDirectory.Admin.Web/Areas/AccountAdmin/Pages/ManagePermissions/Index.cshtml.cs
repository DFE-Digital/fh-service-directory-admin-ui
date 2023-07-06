using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Shared.Models;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Dynamic;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages.ManagePermissions
{
    [Authorize(Roles = $"{RoleTypes.DfeAdmin},{RoleTypes.LaDualRole},{RoleTypes.LaManager}")]
    public class IndexModel : PageModel
    {
        private readonly IIdamClient _idamClient;

        public PaginatedList<AccountDto> PaginatedList { get; set; }

        [BindProperty]
        public int PageNumber { get; set; } = 1;

        [BindProperty]
        public string Name { get; set; } = string.Empty;

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Organisation { get; set; } = string.Empty;

        [BindProperty]
        public bool IsLaUser { get; set; } = false;

        [BindProperty]
        public bool IsVcsUser { get; set; } = false;

        [BindProperty]
        public string SortBy { get; set; } = string.Empty;


        public IndexModel(IServiceDirectoryClient serviceDirectory, IIdamClient idamClient)
        {
            _idamClient = idamClient;
            PaginatedList = new PaginatedList<AccountDto>();
        }

        public async Task OnGet(int? pageNumber, string? name, string? email, string? organisation, bool? isLa, bool? isVcs, string? sortBy)
        {
            ResolveQueryParameters(pageNumber, name, email, organisation, isLa, isVcs, sortBy);

            var users = await _idamClient.GetAccounts(HttpContext.GetUserOrganisationId(), 1, name, email, organisation, isLa, isVcs, sortBy);

            if (users != null)
                PaginatedList = users;
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
            if (pageNumber != null) PageNumber = pageNumber.Value;
            if (name != null) Name = name;
            if (email != null) Email = email;
            if (organisation != null) Organisation = organisation;
            if (isLaUser != null) IsLaUser = isLaUser.Value;
            if (isVcsUser != null) IsVcsUser = isVcsUser.Value;
            if (sortBy != null) SortBy = sortBy;
        }

        private object CreateQueryParameters()
        {
 
            var routeValues = new Dictionary<string, object>();

            routeValues.Add("pageNumber", PageNumber);
            if (Name != null) routeValues.Add("name", Name);
            if (Email != null) routeValues.Add("email", Email);
            if (Organisation != null) routeValues.Add("organisation", Organisation);
            routeValues.Add("isLa", IsLaUser);
            routeValues.Add("isVcs", IsVcsUser);
            routeValues.Add("sortBy", SortBy);

            return routeValues;
        }

        public static string OrganisationName(AccountDto account)
        {
            var organisationName = account?.Claims?.FirstOrDefault(x => x.Name == "OrganisationName")?.Value;
            return organisationName ?? string.Empty;
        }

    }
}
