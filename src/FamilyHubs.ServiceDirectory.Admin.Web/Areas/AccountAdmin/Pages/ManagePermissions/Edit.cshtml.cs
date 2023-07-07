using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Constants;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages.ManagePermissions
{
    public class EditModel : PageModel
    {
        private readonly IIdamClient _idamClient;
        private readonly IServiceDirectoryClient _serviceDirectoryClient;
        private readonly ICacheService _cacheService;

        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Organisation { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string BackPath { get; set; } = "/";

        public EditModel(IIdamClient idamClient, IServiceDirectoryClient serviceDirectoryClient, ICacheService cacheService)
        {
            _idamClient = idamClient;
            _serviceDirectoryClient = serviceDirectoryClient;
            _cacheService = cacheService;
        }

        public async Task OnGet(string email)
        {
            BackPath = await _cacheService.RetrieveLastPageName();
            var account = await _idamClient.GetAccount(email);

            if(account == null)
            {
                throw new Exception("User not found");
            }

            var organisationName = await GetOrganisationName(account);

            Email = email;
            Name = account.Name;
            Organisation = organisationName;
            Role = GetRoleText(account);
        }

        private async Task<string> GetOrganisationName(AccountDto account)
        {
            var organisationClaim = account.Claims.Where(x => x.Name == FamilyHubsClaimTypes.OrganisationId).Single();
            var organisationId = long.Parse(organisationClaim.Value);
            var organisation = await _serviceDirectoryClient.GetOrganisationById(organisationId);

            if(organisation == null)
            {
                throw new Exception("No organisation matches organisationId in claim");
            }

            return organisation.Name;
        }


        private string GetRoleText(AccountDto account)
        {
            var roleClaim = account.Claims.Where(x => x.Name == FamilyHubsClaimTypes.Role).Single();
            var role = roleClaim.Value;

            var typeofPermission = new StringBuilder();

            switch (role)
            {
                case RoleTypes.LaManager:
                    return RoleDescription.LaManager;

                case RoleTypes.LaProfessional:
                    return RoleDescription.LaProfessional;

                case RoleTypes.LaDualRole:
                    return $"{RoleDescription.LaManager}, {RoleDescription.LaProfessional}";

                case RoleTypes.VcsManager:
                    return RoleDescription.VcsManager;

                case RoleTypes.VcsProfessional:
                    return RoleDescription.VcsProfessional;

                case RoleTypes.VcsDualRole:
                    return $"{RoleDescription.VcsManager}, {RoleDescription.VcsProfessional}";
            }

            throw new Exception("Role type not Valid");
        }

    }
}
