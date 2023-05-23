using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;

public class WhichLocalAuthority : AccountAdminViewModel
{
    private readonly IOrganisationAdminClientService _serviceDirectoryClient;

    public WhichLocalAuthority(IOrganisationAdminClientService serviceDirectoryClient)
    {
        PageHeading = "Which local authority is the account for?";
        ErrorMessage = "Select a local authority";
        BackLink = "/TypeOfUserLa";
        _serviceDirectoryClient = serviceDirectoryClient;
    }
    
    [BindProperty]
    public required string LocalAuthority { get; set; } = string.Empty;

    public required IEnumerable<string> Authorities { get; set; } = new List<string>();

    public async Task OnGet()
    {
        await SetDropDownOptions();
    }

    public async Task<IActionResult> OnPost()
    {
        if (ModelState.IsValid && !string.IsNullOrWhiteSpace(LocalAuthority) && LocalAuthority.Length <= 255)
        {
            return RedirectToPage("/UserEmail");
        }
        
        HasValidationError = true;
        await SetDropDownOptions();
        return Page();
    }

    private async Task SetDropDownOptions()
    {
        var localAuthorities = await _serviceDirectoryClient.GetListOrganisations();
        Authorities = localAuthorities.Where(x => x.OrganisationType == Shared.Enums.OrganisationType.LA).Select(x=> x.Name);
    }
}