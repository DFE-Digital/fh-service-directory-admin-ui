using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages.ManagePermissions
{
    public class DeleteUserModel : InputPageViewModel
    {
        private readonly IIdamClient _idamClient;
        private readonly ICacheService _cacheService;

        [BindProperty]
        [Required]
        public bool? DeleteUser { get; set; } = null;
        public string UserName { get; set; } = string.Empty;       

        public DeleteUserModel(IIdamClient idamClient, ICacheService cacheService)
        {
            ErrorMessage = "Select if you want to delete the account";
            SubmitButtonText = "Confirm";                       
            _idamClient = idamClient;
            _cacheService = cacheService;
        }

        public async Task OnGet(long accountId)
        {
            BackButtonPath = await _cacheService.RetrieveLastPageName();
            
            var account = await _idamClient.GetAccountById(accountId);

            if (account == null)
            {
                throw new Exception("User Account not found");
            }

            UserName = account.Name;
            await _cacheService.StoreString("DeleteUserName", UserName);

            PageHeading = $"Do you want to delete {UserName}'s permissions?";
            HintText = $"This will remove all permissions that have been given to {UserName}.";
        }


        public async Task<IActionResult> OnPost(long accountId)
        {
            if (ModelState.IsValid)
            {
                if ( DeleteUser is not null && DeleteUser.Value )
                {
                    await _idamClient.DeleteAccount(accountId);
                    return RedirectToPage("DeleteUserConfirmation", new { AccountId = accountId, IsDeleted = true });
                }
                else
                {
                    return RedirectToPage("DeleteUserConfirmation", new { AccountId = accountId, IsDeleted = false });
                }
                
            }
            UserName = await _cacheService.RetrieveString("DeleteUserName");
            PageHeading = $"Do you want to delete {UserName}'s permissions?";
            HintText = $"This will remove all permissions that have been given to {UserName}.";

            HasValidationError = true;
            return Page();
        }
    }
}
