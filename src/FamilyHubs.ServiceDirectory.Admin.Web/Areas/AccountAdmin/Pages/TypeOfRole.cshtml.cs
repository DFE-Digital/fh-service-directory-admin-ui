using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;

public class TypeOfRole : PageModel
{
    public string RoleForOrganisationType { get; set; } = string.Empty;
    
    public void OnGet()
    {
        
    }
}