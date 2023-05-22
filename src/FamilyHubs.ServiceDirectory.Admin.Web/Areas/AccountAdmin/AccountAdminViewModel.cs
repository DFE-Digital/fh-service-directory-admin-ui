using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin;

public class AccountAdminViewModel : PageModel
{
    public bool HasValidationError { get; set; }

    public string PageHeading { get; set; } = string.Empty;
    
    public string ErrorMessage { get; set; } = string.Empty;
    
    public string BackLink { get; set; } = string.Empty;
}