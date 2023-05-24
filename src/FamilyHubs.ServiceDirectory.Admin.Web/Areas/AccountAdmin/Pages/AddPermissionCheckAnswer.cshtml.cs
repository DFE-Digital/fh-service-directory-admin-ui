using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;

public class AddPermissionCheckAnswer : PageModel
{
    public string WhoFor { get; set; } = string.Empty;
    public string TypeOfPermission { get; set; } = string.Empty;
    public string LocalAuthority { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public void OnGet()
    {
        PopulateAccountDetails();
    }

    private void PopulateAccountDetails()
    {
        WhoFor = "Someone who works for a local authority";
        TypeOfPermission = "Add and manage services, family hubs and accounts";
        LocalAuthority = "Bristol";
        Email = "robert.warpole@firstpm.com";
        Name = "Robert Warpole";
    }
}