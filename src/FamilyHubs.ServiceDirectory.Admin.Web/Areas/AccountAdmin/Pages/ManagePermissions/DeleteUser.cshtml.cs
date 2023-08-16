using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages.ManagePermissions
{
    public class DeleteUserModel : InputPageViewModel
    {
        private readonly IIdamClient _idamClient;
        private readonly ICacheService _cacheService;
        private readonly IEmailService _emailService;

        [BindProperty]
        [Required]
        public bool? DeleteUser { get; set; } = null;
        public string UserName { get; set; } = string.Empty;

        public DeleteUserModel(IIdamClient idamClient, ICacheService cacheService, IEmailService emailService)
        {
            ErrorMessage = "Select if you want to delete the account";
            SubmitButtonText = "Confirm";
            _idamClient = idamClient;
            _cacheService = cacheService;
            _emailService = emailService;
            ErrorElementId = "remove-user";
        }

        public async Task OnGet(long accountId)
        {
            BackButtonPath = await _cacheService.RetrieveLastPageName();

            var account = await _idamClient.GetAccountById(accountId);

            if (account == null)
            {
                throw new ArgumentException("User Account not found");
            }

            UserName = account.Name;
            await _cacheService.StoreString("DeleteUserName", UserName);

            PageHeading = $"Do you want to delete {UserName}'s permissions?";
            HintText = $"This will remove all permissions that have been given to {UserName}.";
        }


        public async Task<IActionResult> OnPost(long accountId)
        {
            if (ModelState.IsValid && DeleteUser is not null)
            {
                if (DeleteUser.Value)
                {
                    var account = await GetAccount(accountId);
                    var role = GetRole(account);
                    var email = new AccountDeletedNotificationModel()
                    {
                        EmailAddress = account!.Email,
                        Role = role,
                    };

                    await _idamClient.DeleteAccount(accountId);
                    await _emailService.SendAccountDeletedEmail(email);

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
        private async Task<AccountDto?> GetAccount(long id)
        {
            var account = await _idamClient.GetAccountById(id);

            if (account is not null)
            {
                return account;
            }

            throw new ArgumentException("User not found");
        }

        private string GetRole(AccountDto? account)
        {
            if (account is not null)
            {
                var roleClaim = account.Claims.Single(x => x.Name == FamilyHubsClaimTypes.Role);
                var role = roleClaim.Value;
                return role;
            }

            throw new ArgumentException("Role not found");
        }
    }
}
