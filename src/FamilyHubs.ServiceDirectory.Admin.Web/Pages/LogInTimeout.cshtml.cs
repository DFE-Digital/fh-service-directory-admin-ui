using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages;

[Authorize]
public class LogInTimeoutModel : HeaderPageModel
{
    public string PreviousPage { get; set; } = string.Empty;
    public void OnGet()
    {
        PreviousPage = Request.Headers["Referer"]!.ToString();
    }
}
