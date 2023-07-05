using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Shared.Models;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages.ManagePermissions
{
    [Authorize(Roles = $"{RoleTypes.DfeAdmin},{RoleTypes.LaDualRole},{RoleTypes.LaManager}")]
    public class IndexModel : PageModel
    {
        private readonly IIdamClient _idamClient;

        public PaginatedList<AccountDto> PaginatedList { get; set; }

        [BindProperty]
        public string Name { get; set; } = string.Empty;

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Organisation { get; set; } = string.Empty;

        [BindProperty]
        public bool IsLocalAuthorityUser { get; set; } = false;

        [BindProperty]
        public bool IsVcsUser { get; set; } = false;

        [BindProperty]
        public string SortBy { get; set; } = string.Empty;

        [BindProperty]
        public SortOrder NameSortOrder { get; set; } = SortOrder.None;

        [BindProperty]
        public SortOrder EmailSortOrder { get; set; } = SortOrder.None;

        [BindProperty]
        public SortOrder OrganisationSortOrder { get; set; } = SortOrder.None;

        public IndexModel(IServiceDirectoryClient serviceDirectory, IIdamClient idamClient)
        {
            _idamClient = idamClient;
            PaginatedList = new PaginatedList<AccountDto>();
        }

        public async Task OnGet()
        {
            var users = await _idamClient.GetAccounts(HttpContext.GetUserOrganisationId(), 1);

            if (users != null)
                PaginatedList = users;
        }

        public async Task OnPost()
        {
            var sortOrder = ResolveSortOrders();
            var users = await _idamClient.GetAccounts(HttpContext.GetUserOrganisationId(), 1, Name, Email, Organisation, IsLocalAuthorityUser, IsVcsUser, sortOrder);

            if (users != null)
                PaginatedList = users;
        }

        private string? ResolveSortOrders()
        {
            if (string.IsNullOrEmpty(SortBy))
            {
                return SortBy;
            }

            switch (SortBy)
            {
                case "Name":
                    NameSortOrder = NewSortOrder(NameSortOrder);
                    EmailSortOrder = SortOrder.None;
                    OrganisationSortOrder = SortOrder.None;
                    return $"Name_{NameSortOrder}";

                case "Email":
                    NameSortOrder = SortOrder.None;
                    EmailSortOrder = NewSortOrder(EmailSortOrder);
                    OrganisationSortOrder = SortOrder.None;
                    return $"Email_{EmailSortOrder}";

                case "Organisation":
                    NameSortOrder = SortOrder.None;
                    EmailSortOrder = SortOrder.None;
                    OrganisationSortOrder = NewSortOrder(OrganisationSortOrder);
                    return $"Organisation_{OrganisationSortOrder}";

            }

            throw new ArgumentException("Invalid value in form post 'SortBy'");
        }

        private SortOrder NewSortOrder(SortOrder currentSortOrder)
        {
            switch (currentSortOrder)
            {
                case SortOrder.Ascending: 
                    return SortOrder.Descending;

                case SortOrder.Descending: 
                    return SortOrder.Ascending;

                default: 
                    return SortOrder.Ascending;
            }
        }

        public static string OrganisationName(AccountDto account)
        {
            var organisationName = account?.Claims?.FirstOrDefault(x => x.Name == "OrganisationName")?.Value;
            return organisationName ?? string.Empty;
        }

        public enum SortOrder
        {
            Ascending,
            Descending,
            None
        }
    }
}
