using System.ComponentModel.DataAnnotations;
using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static FamilyHubs.ServiceDirectory.Admin.Core.Constants.PageConfiguration;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.OrganisationAdmin.Pages;

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

    private readonly IServiceDirectoryClient _serviceDirectoryClient;
    private readonly ICacheService _cacheService;

    public ServiceNameModel(
        IServiceDirectoryClient serviceDirectoryClient, 
        ICacheService cacheService)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
        _cacheService = cacheService;
    }

    public async Task OnGet(long organisationId, long serviceId, string strOrganisationViewModel)
    {
        OrganisationId = organisationId;
        LastPage = await _cacheService.RetrieveLastPageName();
        UserFlow = await _cacheService.RetrieveUserFlow();

        var sessionVm = await _cacheService.RetrieveOrganisationWithService();
        if (sessionVm != null && organisationId < 1) 
        {
            OrganisationId = sessionVm.Id;
        }

        if (sessionVm != default)
            ServiceName = sessionVm.ServiceName ?? "";
        
        if(sessionVm?.Uri == default)
        {
            var organisation = await _serviceDirectoryClient.GetOrganisationById(organisationId);

            if (organisation is null)
                throw new Exception($"Organisation id {organisationId} not found");

            var apiVm = ApiModelToViewModelHelper.CreateViewModel(organisation, serviceId);
            
            if (!string.IsNullOrEmpty(apiVm.ServiceName))
                ServiceName = apiVm.ServiceName;
                
            await _cacheService.StoreOrganisationWithService(apiVm);
        }
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid || ServiceName is null || ServiceName.Trim().Length == 0 || ServiceName.Length > 255)
        {
            ValidationValid = false;
            return Page();
        }
        
        var sessionVm = await _cacheService.RetrieveOrganisationWithService() ?? new OrganisationViewModel();

        sessionVm.ServiceName = ServiceName;
        await _cacheService.StoreOrganisationWithService(sessionVm);

        return RedirectToPage(await _cacheService.RetrieveLastPageName() == CheckServiceDetailsPageName 
            ? $"/OrganisationAdmin/{CheckServiceDetailsPageName}" 
            : "/OrganisationAdmin/TypeOfService");
    }
}
