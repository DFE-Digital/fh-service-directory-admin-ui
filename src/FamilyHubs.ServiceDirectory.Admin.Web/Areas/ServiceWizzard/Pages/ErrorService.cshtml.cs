using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.ServiceWizzard.Pages;

public class ErrorServiceModel : HeaderPageModel
{
    public IActionResult OnPost()
    {
        return RedirectToPage("Welcome", new { area = "ServiceWizzard" });
    }
}
