using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.ServiceWizzard.Pages;

public class ServiceNameModel : PageModel
{
    public string OrganisationId { get; set; } = default!;

    [BindProperty]
    [Required(ErrorMessage = "You must enter a service name")]
    public string ServiceName { get; set; } = default!;

    [BindProperty]
    public bool ValidationValid { get; set; } = true;

    private readonly IConnectionRequestDistributedCache _connectionRequestCache;

    public ServiceNameModel(IConnectionRequestDistributedCache connectionRequestCache)
    {
        _connectionRequestCache = connectionRequestCache;
    }

    public async Task OnGet(string organisationid, string serviceid, string strOrganisationViewModel)
    {
        var user = HttpContext.GetFamilyHubsUser();
        ConnectionRequestModel? connectionRequestModel = await _connectionRequestCache.GetAsync(user.Email);
        if (connectionRequestModel != null)
        {
            ServiceName = connectionRequestModel.ServiceName;
        }
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid || ServiceName == null || ServiceName.Trim().Length == 0 || ServiceName.Length > 255)
        {
            ValidationValid = false;
            return Page();
        }

        var user = HttpContext.GetFamilyHubsUser();
        ConnectionRequestModel? connectionRequestModel = await _connectionRequestCache.GetAsync(user.Email);
        if (connectionRequestModel == null)
        {
            connectionRequestModel = new ConnectionRequestModel();
        }
        connectionRequestModel.ServiceName = ServiceName;
        await _connectionRequestCache.SetAsync(user.Email, connectionRequestModel);

        return RedirectToPage("TypeOfSupport", new { area = "ServiceWizzard" });

    }
}
