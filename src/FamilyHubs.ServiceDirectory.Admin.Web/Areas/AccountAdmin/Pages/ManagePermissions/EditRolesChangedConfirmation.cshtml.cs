using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages.ManagePermissions
{
    public class EditRolesChangedConfirmationModel : PageModel
    {
        private readonly IIdamClient _idamClient;

        [BindProperty(SupportsGet = true)]
        public string AccountId { get; set; } = string.Empty; //Route Property

        public string UserName { get; set; } = string.Empty;

        public EditRolesChangedConfirmationModel(IIdamClient idamClient)
        {
            _idamClient = idamClient;
        }

        public async Task OnGet()
        {
            var id = GetAccountId();
            var account = await _idamClient.GetAccountById(id);

            if (account == null)
            {
                throw new Exception("User Account not found");
            }

            UserName = account.Name;
        }

        private long GetAccountId()
        {
            if (long.TryParse(AccountId, out long id))
            {
                return id;
            }

            throw new Exception("Invalid AccountId");
        }
    }
}
