using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.OrganisationAdmin;

public class WelcomeModel : PageModel
{
    [BindProperty]
    public OrganisationViewModel OrganisationViewModel { get; set; } = new OrganisationViewModel();

    public bool IsUploadSpreadsheetEnabled { get; private set; }

    private readonly IRedisCacheService _redis;
    private readonly IOrganisationAdminClientService _organisationAdminClientService;

    public List<ServiceDto> Services { get; private set; } = default!;

    public string LastPage { get; private set; } = default!;

    public WelcomeModel(
        IRedisCacheService redisCacheService, 
        IOrganisationAdminClientService organisationAdminClientService,
        IConfiguration configuration)
    {
        _redis = redisCacheService;
        _organisationAdminClientService = organisationAdminClientService;
        IsUploadSpreadsheetEnabled = configuration.GetValue<bool>("IsUploadSpreadsheetEnabled");
    }

    public async Task OnGet(long? organisationId)
    {
        //TODO - get organisation id from redis rather than passing in as parameter, get the org id from redis if available, then reset
        LastPage = $"/OrganisationAdmin/{_redis.RetrieveLastPageName()}";

        _redis.ResetOrganisationWithService();

        if (_redis.RetrieveOrganisationWithService() == null)
        {
            OrganisationWithServicesDto? organisation = null;

            if (organisationId.HasValue)
            {
                organisation = await _organisationAdminClientService.GetOrganisationById(organisationId.Value);
            }

            if (organisation != null)
            {
                OrganisationViewModel = new OrganisationViewModel
                {
                    Id = organisation.Id,
                    Name = organisation.Name
                };

                _redis.StoreOrganisationWithService(OrganisationViewModel);
            }
            else //TODO - don't have a default LA
            {
                throw new NotImplementedException();
            }
        }
        else
        {
            OrganisationViewModel = _redis.RetrieveOrganisationWithService() ?? new OrganisationViewModel();
        }

        Services = await _organisationAdminClientService.GetServicesByOrganisationId(OrganisationViewModel.Id);

        _redis.ResetLastPageName();
    }

    public IActionResult OnGetAddServiceFlow(string organisationId, string serviceId, string strOrganisationViewModel)
    {
        _redis.StoreUserFlow("AddService");
        return RedirectToPage("/OrganisationAdmin/ServiceName", new { organisationId });
    }

    public IActionResult OnGetManageServiceFlow(string organisationId)
    {
        _redis.StoreUserFlow("ManageService");
        return RedirectToPage("/OrganisationAdmin/ViewServices", new { organisationId });
    }

    public IActionResult OnGetUploadSpreadsheetData(string organisationId)
    {
        _redis.StoreUserFlow("UploadSpreadsheetData");
        return RedirectToPage("/OrganisationAdmin/UploadSpreadsheetData", new { organisationId });
    }
}
