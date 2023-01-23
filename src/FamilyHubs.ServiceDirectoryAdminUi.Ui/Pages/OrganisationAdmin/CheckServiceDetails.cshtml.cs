using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralTaxonomys;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using FamilyHubs.SharedKernel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static FamilyHubs.ServiceDirectoryAdminUi.Ui.Infrastructure.Configuration.PageConfiguration;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

/*
For Testing
Json = {"Id":"72e653e8-1d05-4821-84e9-9177571a6013","Name":"Bristol City Council","Description":"Bristol City Council","Logo":null,"Uri":null,"Url":"https://www.bristol.gov.uk/","ServiceName":"Happy Babies","ServiceDescription":"Some description","TaxonomySelection":["bccagegroup:37","bccprimaryservicetype:38"],"ServiceDeliverySelection":["1","2"],"InPersonSelection":["Our own location"],"WhoForSelection":["Children"],"Languages":["English"],"MinAge":1,"MaxAge":5,"Address_1":"Address line 1","City":"Town","Postal_code":"Postcode","Country":"England","State_province":"County","IsPayedFor":"Yes","PayUnit":"Session","Cost":12.55,"ContactSelection":["email","phone"],"Email":"someone@aol.com","Telephone":"Telephone1","Website":null}
 
URL = https://localhost:7177/OrganisationAdmin/CheckServiceDetails?strOrganisationViewModel=%7B%22Id%22%3A%2272e653e8-1d05-4821-84e9-9177571a6013%22,%22Name%22%3A%22Bristol%20City%20Council%22,%22Description%22%3A%22Bristol%20County%20Council%22,%22Logo%22%3Anull,%22Uri%22%3Anull,%22Url%22%3A%22https%3A%2F%2Fwww.bristol.gov.uk%2F%22,%22ServiceName%22%3A%22Happy%20Babies%22,%22ServiceDescription%22%3A%22Some%20description%22,%22TaxonomySelection%22%3A%5B%22bccagegroup%3A37%22,%22bccprimaryservicetype%3A38%22%5D,%22ServiceDeliverySelection%22%3A%5B%221%22,%222%22%5D,%22InPersonSelection%22%3A%5B%22Our%20own%20location%22%5D,%22WhoForSelection%22%3A%5B%22Children%22%5D,%22Languages%22%3A%5B%22English%22%5D,%22MinAge%22%3A1,%22MaxAge%22%3A5,%22Address_1%22%3A%22Address%20line%201%22,%22City%22%3A%22Town%22,%22Postal_code%22%3A%22Postcode%22,%22Country%22%3A%22England%22,%22State_province%22%3A%22County%22,%22IsPayedFor%22%3A%22Yes%22,%22PayUnit%22%3A%22Session%22,%22Cost%22%3A12.55,%22ContactSelection%22%3A%5B%22email%22,%22phone%22%5D,%22Email%22%3A%22someone@aol.com%22,%22Telephone%22%3A%22Telephone1%22,%22Website%22%3Anull%7D
 
 */

public class CheckServiceDetailsModel : PageModel
{
    public List<string> ServiceDeliverySelection { get; set; } = new List<string>();
    public List<OpenReferralTaxonomyDto> SelectedTaxonomy { get; set; } = new List<OpenReferralTaxonomyDto>();
    public OrganisationViewModel OrganisationViewModel { get; set; } = default!;
    public string UserFlow { get; set; } = default!;
    public string Address_1 { get; set; } = default!;
    public string Address_2 { get; set; } = default!;
    public string? Cost { get; set; }

    private readonly IOpenReferralOrganisationAdminClientService _openReferralOrganisationAdminClientService;
    private readonly IViewModelToApiModelHelper _viewModelToApiModelHelper;
    private readonly ISessionService _session;
    private readonly IRedisCacheService _redis;

    public CheckServiceDetailsModel(IOpenReferralOrganisationAdminClientService openReferralOrganisationAdminClientService,
                                    IViewModelToApiModelHelper viewModelToApiModelHelper, 
                                    ISessionService sessionService,
                                    IRedisCacheService redisCacheService)
    {
        _openReferralOrganisationAdminClientService = openReferralOrganisationAdminClientService;
        _viewModelToApiModelHelper = viewModelToApiModelHelper;
        _session = sessionService;
        _redis = redisCacheService;
    }

    private async Task InitPage()
    {
        _redis.StoreCurrentPageName("CheckServiceDetails");
        OrganisationViewModel = _redis.RetrieveOrganisationWithService() ?? new OrganisationViewModel();
        SplitAddressFields();
        Cost = string.Format("{0:0.00}", OrganisationViewModel?.Cost);
        PaginatedList<OpenReferralTaxonomyDto> taxonomies = await _openReferralOrganisationAdminClientService.GetTaxonomyList(1, 9999);

        if (taxonomies != null && OrganisationViewModel != null && OrganisationViewModel.TaxonomySelection != null)
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

        var myEnumDescriptions = from ServiceDelivery n in Enum.GetValues(typeof(ServiceDelivery))
                                 select new { Id = (int)n, Name = Utility.GetEnumDescription(n) };

        Dictionary<int, string> dictServiceDelivery = new();
        foreach (var myEnumDescription in myEnumDescriptions)
        {
            if (myEnumDescription.Id == 0)
                continue;
            dictServiceDelivery[myEnumDescription.Id] = myEnumDescription.Name;
        }

        if (OrganisationViewModel != null && OrganisationViewModel.ServiceDeliverySelection != null)
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

        if (organisationViewModel != null)
        {
            var result = string.Empty;
            var openReferralOrganisationWithServicesRecord = await _viewModelToApiModelHelper.GetOrganisation(organisationViewModel);
            if (openReferralOrganisationWithServicesRecord != null)
            {
                var service = openReferralOrganisationWithServicesRecord?.Services?.FirstOrDefault();
                if (service != null)
                {
                    organisationViewModel.ServiceId = service.Id;
                }
            }

            if (openReferralOrganisationWithServicesRecord != null)
            {
                if (organisationViewModel.Id == Guid.Empty)
                {
                    result = await _openReferralOrganisationAdminClientService.CreateOrganisation(openReferralOrganisationWithServicesRecord);
                }
                else
                {
                    result = await _openReferralOrganisationAdminClientService.UpdateOrganisation(openReferralOrganisationWithServicesRecord);
                }
            }

            if (!string.IsNullOrEmpty(result))
                
                _redis.StoreOrganisationWithService(organisationViewModel);
        }

        _redis.StoreCurrentPageName(null);
        _redis.StoreOrganisationWithService(null); //TODO - Use session.clear instead of this
        
        UserFlow = _redis.RetrieveUserFlow();
        switch (UserFlow)
        {
            case "ManageService":
                return RedirectToPage("/OrganisationAdmin/DetailsSaved");
               
            default:
                return RedirectToPage("/OrganisationAdmin/ServiceAdded");
               
        }

    }

    public IActionResult OnGetRedirectToViewServicesPage(string orgId)
    {
        return RedirectToPage("/OrganisationAdmin/ViewServices", 
                                new
                                {
                                    orgId
                                });
    }

    private void SplitAddressFields()
    {
        if (string.IsNullOrEmpty(OrganisationViewModel.Address_1))
            return;

        var modelAddress1 = OrganisationViewModel.Address_1;
        
        Address_1 = modelAddress1;
        Address_2 = String.Empty;

        if (modelAddress1.Contains("|"))
        {
            Address_1 = modelAddress1.Substring(0, modelAddress1.IndexOf("|"));
            Address_2 = modelAddress1.Substring(modelAddress1.IndexOf("|") + 1);
        }

    }
}
