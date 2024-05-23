using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Errors;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using FamilyHubs.SharedKernel.Razor.ErrorNext;
using FamilyHubs.SharedKernel.Razor.FullPages.Radios;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages.ManagePermissions
{
    public class DeleteUserModel : PageModel, IRadiosPageModel
    {
        private readonly IIdamClient _idamClient;
        private readonly ICacheService _cacheService;
        private readonly IEmailService _emailService;

        public string Legend => $"Do you want to delete {UserName}'s permissions?";
        public string Hint => $"This will remove all permissions that have been given to {UserName}.";
        public string ButtonText => "Confirm";
        public string BackUrl { get; set; } = "/";

        public IEnumerable<IRadio> Radios => new Radio[]
        {
            new("Yes, delete the permissions", bool.TrueString),
            new("No, keep the permissions", bool.FalseString)
        };

        public IErrorState Errors { get; protected set; } = ErrorState.Empty;

        [BindProperty]
        public string? SelectedValue { get; set; }

        public bool? DeleteUser {
            get => bool.TryParse(SelectedValue, out var result) ? result : null;
            set => SelectedValue = value.ToString();
        }
        public string UserName { get; set; } = string.Empty;

        public DeleteUserModel(IIdamClient idamClient, ICacheService cacheService, IEmailService emailService)
        {
            _idamClient = idamClient;
            _cacheService = cacheService;
            _emailService = emailService;
        }

        public async Task OnGet(long accountId)
        {
            BackUrl = await _cacheService.RetrieveLastPageName();

            var account = await _idamClient.GetAccountById(accountId);

            if (account == null)
            {
                throw new ArgumentException("User Account not found");
            }

            UserName = account.Name;
            await _cacheService.StoreString("DeleteUserName", UserName);
        }


        public async Task<IActionResult> OnPost(long accountId)
        {
            if (ModelState.IsValid && DeleteUser is not null)
            {
                if (!DeleteUser.Value)
                {
                    return RedirectToPage("DeleteUserConfirmation", new {AccountId = accountId, IsDeleted = false});
                }

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
            UserName = await _cacheService.RetrieveString("DeleteUserName");
            Errors = ErrorState.Create(PossibleErrors.All, ErrorId.ManagePermissions_Delete_MissingSelection);
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
            if (account is null) throw new ArgumentException("Role not found");

            var roleClaim = account.Claims.Single(x => x.Name == FamilyHubsClaimTypes.Role);
            var role = roleClaim.Value;
            return role;
        }
    }
}
