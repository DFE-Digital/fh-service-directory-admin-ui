using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Shared.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages.ManagePermissions
{
    public class PaginatedResultsModel : PageModel
    {
        public PaginatedList<AccountDto> PaginatedList { get; set; }

        public PaginatedResultsModel(PaginatedList<AccountDto>? paginatedList)
        {
            PaginatedList = paginatedList ?? new PaginatedList<AccountDto>();
        }

        public void OnGet()
        {
        }
    }
}
