using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static FamilyHubs.ServiceDirectory.Admin.Core.Constants.PageConfiguration;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.OrganisationAdmin.Pages;

public class CheckServiceDetailsModel : PageModel
{
    public List<string> ServiceDeliverySelection { get; set; } = new List<string>();
    public List<TaxonomyDto> SelectedTaxonomy { get; set; } = new List<TaxonomyDto>();
    public OrganisationViewModel OrganisationViewModel { get; set; } = default!;
    public string UserFlow { get; set; } = default!;
    public string Address1 { get; set; } = default!;
    public string Address2 { get; set; } = default!;
    public string? Cost { get; set; }

    private readonly IOrganisationAdminClientService _organisationAdminClientService;
    private readonly IViewModelToApiModelHelper _viewModelToApiModelHelper;
    private readonly ICacheService _cacheService;

    public CheckServiceDetailsModel(
        IOrganisationAdminClientService organisationAdminClientService,
        IViewModelToApiModelHelper viewModelToApiModelHelper,
        ICacheService cacheService)
    {
        _organisationAdminClientService = organisationAdminClientService;
        _viewModelToApiModelHelper = viewModelToApiModelHelper;
        _cacheService = cacheService;
    }

    private async Task InitPage()
    {
        _cacheService.StoreCurrentPageName("CheckServiceDetails");
        OrganisationViewModel = _cacheService.RetrieveOrganisationWithService() ?? new OrganisationViewModel();
        SplitAddressFields();
        Cost = $"{OrganisationViewModel.Cost:0.00}";
        var taxonomies = await _organisationAdminClientService.GetTaxonomyList(1, 9999);

        if (OrganisationViewModel.TaxonomySelection != null)
        {
            foreach (var taxonomyKey in OrganisationViewModel.TaxonomySelection)
            {
                var taxonomy = taxonomies.Items.FirstOrDefault(x => x.Id == taxonomyKey);
                if (taxonomy != null)
                {
                    SelectedTaxonomy.Add(taxonomy);
                }
            }
        }

        var myEnumDescriptions = from ServiceDeliveryType n in Enum.GetValues(typeof(ServiceDeliveryType))
                                 select new { Id = (int)n, Name = Utility.GetEnumDescription(n) };

        var dictServiceDelivery = new Dictionary<int, string>();
        foreach (var myEnumDescription in myEnumDescriptions)
        {
            if (myEnumDescription.Id == 0)
                continue;
            dictServiceDelivery[myEnumDescription.Id] = myEnumDescription.Name;
        }

        if (OrganisationViewModel.ServiceDeliverySelection != null)
        {
            foreach (var item in OrganisationViewModel.ServiceDeliverySelection)
            {
                if (int.TryParse(item, out var value))
                {
                    ServiceDeliverySelection.Add(dictServiceDelivery[value]);
                }
            }
        }
    }

    public async Task<IActionResult> OnGet()
    {
        UserFlow = _cacheService.RetrieveUserFlow();

        if (_cacheService.RetrieveLastPageName() == ServiceAddedPageName)
        {
            return RedirectToPage("/OrganisationAdmin/ErrorService");
        }

        await InitPage();

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        var organisationViewModel = _cacheService.RetrieveOrganisationWithService() ?? new OrganisationViewModel();

        if (organisationViewModel.ServiceId is null or <= 0)
        {
            var serviceDto = _viewModelToApiModelHelper.MapViewModelToDto(organisationViewModel);
            organisationViewModel.ServiceId = await _organisationAdminClientService.CreateService(serviceDto);
        }
        else
        {
            var serviceDto = await _viewModelToApiModelHelper.GenerateUpdateServiceDto(organisationViewModel);
            await _organisationAdminClientService.UpdateService(serviceDto);
        }

        _cacheService.ResetLastPageName();

        UserFlow = _cacheService.RetrieveUserFlow();
        
        return UserFlow switch
        {
            "ManageService" => RedirectToPage("/OrganisationAdmin/DetailsSaved"),
            _ => RedirectToPage("/OrganisationAdmin/ServiceAdded")
        };
    }

    public IActionResult OnGetRedirectToViewServicesPage(string orgId)
    {
        return RedirectToPage("/OrganisationAdmin/ViewServices", new { orgId });
    }

    private void SplitAddressFields()
    {
        if (string.IsNullOrEmpty(OrganisationViewModel.Address1))
            return;

        var modelAddress1 = OrganisationViewModel.Address1;

        Address1 = modelAddress1;
        Address2 = string.Empty;

        if (!modelAddress1.Contains("|")) return;

        Address1 = modelAddress1.Substring(0, modelAddress1.IndexOf("|", StringComparison.Ordinal));
        Address2 = modelAddress1.Substring(modelAddress1.IndexOf("|", StringComparison.Ordinal) + 1);
    }
}
