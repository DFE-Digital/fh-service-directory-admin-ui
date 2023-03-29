using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static FamilyHubs.ServiceDirectory.Admin.Core.Constants.PageConfiguration;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.OrganisationAdmin;

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
    private readonly IRedisCacheService _redis;

    public CheckServiceDetailsModel(
        IOrganisationAdminClientService organisationAdminClientService,
        IViewModelToApiModelHelper viewModelToApiModelHelper,
        IRedisCacheService redisCacheService)
    {
        _organisationAdminClientService = organisationAdminClientService;
        _viewModelToApiModelHelper = viewModelToApiModelHelper;
        _redis = redisCacheService;
    }

    private async Task InitPage()
    {
        _redis.StoreCurrentPageName("CheckServiceDetails");
        OrganisationViewModel = _redis.RetrieveOrganisationWithService() ?? new OrganisationViewModel();
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
        UserFlow = _redis.RetrieveUserFlow();

        if (_redis.RetrieveLastPageName() == ServiceAddedPageName)
        {
            return RedirectToPage("/OrganisationAdmin/ErrorService");
        }

        await InitPage();

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        var organisationViewModel = _redis.RetrieveOrganisationWithService() ?? new OrganisationViewModel();

        if (organisationViewModel.ServiceId is null or < 1)
        {
            var serviceDto = _viewModelToApiModelHelper.MapViewModelToDto(organisationViewModel);
            organisationViewModel.ServiceId = await _organisationAdminClientService.CreateService(serviceDto);
        }
        else
        {
            var serviceDto = await _viewModelToApiModelHelper.GenerateUpdateServiceDto(organisationViewModel);
            await _organisationAdminClientService.UpdateService(serviceDto);
        }

        _redis.StoreOrganisationWithService(organisationViewModel);
        _redis.StoreCurrentPageName(null);
        _redis.StoreOrganisationWithService(null); //TODO - Use session.clear instead of this

        UserFlow = _redis.RetrieveUserFlow();
        
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
