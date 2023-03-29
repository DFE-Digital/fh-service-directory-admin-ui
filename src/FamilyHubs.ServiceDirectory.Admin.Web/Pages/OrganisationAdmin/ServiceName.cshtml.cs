using System.ComponentModel.DataAnnotations;
using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static FamilyHubs.ServiceDirectory.Admin.Core.Constants.PageConfiguration;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.OrganisationAdmin;

public class ServiceNameModel : PageModel
{
    public string LastPage { get; set; } = default!;
    public string UserFlow { get; set; } = default!;
    public long OrganisationId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "You must enter a service name")]
    public string? ServiceName { get; set; } = default!;

    [BindProperty]
    public bool ValidationValid { get; set; } = true;

    private readonly IOrganisationAdminClientService _organisationAdminClientService;
    private readonly IRedisCacheService _redis;

    public ServiceNameModel(
        IOrganisationAdminClientService organisationAdminClientService, 
        IRedisCacheService redisCacheService)
    {
        _organisationAdminClientService = organisationAdminClientService;
        _redis = redisCacheService;
    }

    public async Task OnGet(long organisationId, long serviceId, string strOrganisationViewModel)
    {
        OrganisationId = organisationId;
        LastPage = _redis.RetrieveLastPageName();
        UserFlow = _redis.RetrieveUserFlow();

        var sessionVm = _redis.RetrieveOrganisationWithService();
        if (sessionVm != null && organisationId < 1) 
        {
            OrganisationId = sessionVm.Id;
        }

        if (sessionVm != default)
            ServiceName = sessionVm.ServiceName ?? "";
        
        if(sessionVm?.Uri == default)
        {
            var organisation = await _organisationAdminClientService.GetOrganisationById(organisationId);

            if (organisation is null)
                throw new Exception($"Organisation id {organisationId} not found");

            var apiVm = ApiModelToViewModelHelper.CreateViewModel(organisation, serviceId);
            
            if (!string.IsNullOrEmpty(apiVm.ServiceName))
                ServiceName = apiVm.ServiceName;
                
            _redis.StoreOrganisationWithService(apiVm);
        }
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid || ServiceName is null || ServiceName.Trim().Length == 0 || ServiceName.Length > 255)
        {
            ValidationValid = false;
            return Page();
        }
        
        var sessionVm = _redis.RetrieveOrganisationWithService() ?? new OrganisationViewModel();

        sessionVm.ServiceName = ServiceName;
        _redis.StoreOrganisationWithService(sessionVm);

        return RedirectToPage(_redis.RetrieveLastPageName() == CheckServiceDetailsPageName 
            ? $"/OrganisationAdmin/{CheckServiceDetailsPageName}" 
            : "/OrganisationAdmin/TypeOfService");
    }
}
