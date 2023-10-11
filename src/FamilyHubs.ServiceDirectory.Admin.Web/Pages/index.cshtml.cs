using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Razor.FamilyHubsUi.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages;

public class IndexModel : HeaderPageModel
{
    private readonly FamilyHubsUiOptions _familyHubsUiOptions;

    public string ConnectUrl { get; set; } = string.Empty;
    public string FindUrl { get; set; } = string.Empty;

    public IndexModel(IOptions<FamilyHubsUiOptions> configuration)
    {
        _familyHubsUiOptions = configuration.Value;
    }
    public IActionResult OnGet()
    {
        if (HttpContext.IsUserLoggedIn())
        {
            return RedirectToPage("/Welcome");
        }

        ConnectUrl = _familyHubsUiOptions.Url(UrlKeys.ConnectWeb).ToString();
        FindUrl = _familyHubsUiOptions.Url(UrlKeys.FindWeb).ToString();

        return Page();
    }
}