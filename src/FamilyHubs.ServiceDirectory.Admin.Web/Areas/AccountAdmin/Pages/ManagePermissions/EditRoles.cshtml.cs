using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Helpers;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Mail;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages.ManagePermissions
{
    public class EditRolesModel : InputPageViewModel
    {
        private readonly IIdamClient _idamClient;

        [BindProperty(SupportsGet = true)]
        public string AccountId { get; set; } = string.Empty; //Route Property               

        [BindProperty]
        public bool LaProfessional { get; set; }

        [BindProperty]
        public bool LaManager { get; set; }

        [BindProperty]
        public bool VcsProfessional { get; set; }

        [BindProperty]
        public bool VcsManager { get; set; }


        public bool IsLa { get; set; } = false;


        public bool IsVcs { get; set; } = false;

        public EditRolesModel(IIdamClient idamClient)
        {
            PageHeading = "What do they need to do?";
            ErrorMessage = "Select what they need to do";
            SubmitButtonText = "Confirm";
            _idamClient = idamClient;
        }

        public async Task OnGet()
        {
            BackButtonPath = $"/AccountAdmin/ManagePermissions/{AccountId}";

            var id = GetAccountId();
            var account = await GetAccount(id);
            string role = GetRole(account);
            SetOrganisationType(role);
            SetRoleSelection(role);
        }


        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid && (LaManager || LaProfessional || VcsManager || VcsProfessional))
            {
                var accountId = GetAccountId();

                var newRole = GetSelectedRole();
                var request = new UpdateClaimDto { AccountId = accountId, Name = "role", Value = newRole };
                await _idamClient.UpdateClaim(request);

                return RedirectToPage("/placeholder");
            }

            HasValidationError = true;

            var id = GetAccountId();
            var account = await GetAccount(id);
            string role = GetRole(account);
            SetOrganisationType(role);

            return Page();
        }

        private string GetSelectedRole()
        {
            var role = string.Empty;
            //La
            if (LaManager == true && LaProfessional == true)
            {
                role = RoleTypes.LaDualRole;
            }
            else if (LaManager == true)
            {
                role = RoleTypes.LaManager;
            }
            else if (LaProfessional == true)
            {
                role = RoleTypes.LaProfessional;
            }
            //Vcs
            else if (VcsManager == true && VcsProfessional == true)
            {
                role = RoleTypes.VcsDualRole;
            }
            else if (VcsManager == true)
            {
                role = RoleTypes.VcsManager;
            }
            else if (VcsProfessional == true)
            {
                role = RoleTypes.VcsProfessional;
            }



            return role;
        }

        private void SetOrganisationType(string role)
        {
            if (role == RoleTypes.LaManager || role == RoleTypes.LaProfessional || role == RoleTypes.LaDualRole)
            {
                IsLa = true;
            }
            //Vcs
            else if (role == RoleTypes.VcsManager || role == RoleTypes.VcsProfessional || role == RoleTypes.VcsDualRole)
            {
                IsVcs = true;
            }
        }

        private void SetRoleSelection(string role)
        {
            //La
            if (role == RoleTypes.LaManager)
            {
                LaManager = true;
            }
            else if (role == RoleTypes.LaProfessional)
            {
                LaProfessional = true;
            }
            else if (role == RoleTypes.LaDualRole)
            {
                LaManager = true;
                LaProfessional = true;
            }
            //Vcs
            else if (role == RoleTypes.VcsManager)
            {
                VcsManager = true;
            }
            else if (role == RoleTypes.VcsProfessional)
            {
                VcsProfessional = true;
            }
            else if (role == RoleTypes.VcsDualRole)
            {
                VcsManager = true;
                VcsProfessional = true;
            }
        }

        private long GetAccountId()
        {
            if (long.TryParse(AccountId, out long id))
            {
                return id;
            }

            throw new Exception("Invalid AccountId");
        }

        private async Task<AccountDto?> GetAccount(long id)
        {
            var account = await _idamClient.GetAccountById(id);

            if (account is not null)
            {
                return account;
            }

            throw new Exception("User not found");
        }

        private string GetRole(AccountDto? account)
        {
            if (account is not null)
            {
                var roleClaim = account.Claims.Where(x => x.Name == FamilyHubsClaimTypes.Role).Single();
                var role = roleClaim.Value;
                return role;
            }

            throw new Exception("Role not found");
        }
    }
}
