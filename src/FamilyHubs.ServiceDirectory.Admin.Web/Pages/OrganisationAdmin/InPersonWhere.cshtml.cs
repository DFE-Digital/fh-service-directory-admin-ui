using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static FamilyHubs.ServiceDirectory.Admin.Core.Constants.PageConfiguration;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.OrganisationAdmin;

public class InPersonWhereModel : PageModel
{
    public string LastPage { get; set; } = default!;
    public string UserFlow { get; set; } = default!;

    [BindProperty]
    public string Address1 { get; set; } = default!;
    [BindProperty]
    public string Address2 { get; set; } = default!;
    [BindProperty]
    public string City { get; set; } = default!;
    [BindProperty]
    public string PostalCode { get; set; } = default!;
    [BindProperty]
    public string Country { get; set; } = default!;
    [BindProperty]
    public string StateProvince { get; set; } = default!;

    [BindProperty]
    public List<string> InPersonSelection { get; set; } = default!;

    [BindProperty]
    public OrganisationViewModel OrganisationViewModel { get; set; } = new OrganisationViewModel();

    [BindProperty]
    public bool ValidationValid { get; set; } = true;

    [BindProperty]
    public bool Address1Valid { get; set; } = true;

    [BindProperty]
    public bool TownCityValid { get; set; } = true;

    [BindProperty]
    public bool PostcodeValid { get; set; } = true;

    [BindProperty]
    public bool PostcodeApiValid { get; set; } = true;

    private readonly IPostcodeLocationClientService _postcodeLocationClientService;
    private readonly ICacheService _cacheService;

    public InPersonWhereModel(
        IPostcodeLocationClientService postcodeLocationClientService, 
        ICacheService cacheService)
    {
        _postcodeLocationClientService = postcodeLocationClientService;
        _cacheService = cacheService;
    }

    public void OnGet(string strOrganisationViewModel)
    {
        LastPage = _cacheService.RetrieveLastPageName();
        UserFlow = _cacheService.RetrieveUserFlow();

        OrganisationViewModel = _cacheService.RetrieveOrganisationWithService() ?? new OrganisationViewModel();

        OrganisationViewModel.Country = "England";

        if (!string.IsNullOrEmpty(OrganisationViewModel.Address1))
            if (OrganisationViewModel.Address1.Split("|").Length > 1)
            {
                Address1 = OrganisationViewModel.Address1.Split("|")[0];
                Address2 = OrganisationViewModel.Address1.Split("|")[1];
            }
            else
            {
                Address1 = OrganisationViewModel.Address1;
            }
        if (!string.IsNullOrEmpty(OrganisationViewModel.City))
            City = OrganisationViewModel.City;
        if (!string.IsNullOrEmpty(OrganisationViewModel.PostalCode))
            PostalCode = OrganisationViewModel.PostalCode;
        if (!string.IsNullOrEmpty(OrganisationViewModel.StateProvince))
            StateProvince = OrganisationViewModel.StateProvince;
        if (OrganisationViewModel.InPersonSelection != null && OrganisationViewModel.InPersonSelection.Any())
            InPersonSelection = OrganisationViewModel.InPersonSelection;
    }

    public async Task<IActionResult> OnPost()
    {
        ModelState.Remove("Country");
        ModelState.Remove("Address_2");
        ModelState.Remove("State_province");
        ModelState.Remove("PostcodeAPIValid");
        Country = "England";

        try
        {
            var _ = await _postcodeLocationClientService.LookupPostcode(PostalCode);
        }
        catch
        {
            PostcodeApiValid = false;
        }

        if (!ModelState.IsValid ||
            string.IsNullOrEmpty(Address1) ||
            string.IsNullOrEmpty(City) ||
            string.IsNullOrEmpty(PostalCode) ||
            PostcodeApiValid == false)
        {
            ValidationValid = false;

            if (string.IsNullOrEmpty(Address1))
                Address1Valid = false;

            if (string.IsNullOrEmpty(City))
                TownCityValid = false;

            if (string.IsNullOrEmpty(PostalCode) || PostcodeApiValid == false ||
                PostalCode.Replace(" ", string.Empty).Length is < 6 or > 7)
                PostcodeValid = false;

            return Page();
        }

        if (!string.IsNullOrEmpty(PostalCode))
            InPersonSelection.Add("Our own location");

        OrganisationViewModel = _cacheService.RetrieveOrganisationWithService() ?? new OrganisationViewModel();
        OrganisationViewModel.InPersonSelection = new List<string>(InPersonSelection);
        OrganisationViewModel.Address1 = Address1 + "|" + Address2;
        OrganisationViewModel.City = City;
        OrganisationViewModel.StateProvince = StateProvince;
        OrganisationViewModel.Country = "England";
        OrganisationViewModel.PostalCode = PostalCode;

        if (!string.IsNullOrEmpty(PostalCode))
        {
            var postcodeApiModel = await _postcodeLocationClientService.LookupPostcode(PostalCode);
            
            OrganisationViewModel.Latitude = postcodeApiModel.Result.Latitude;
            OrganisationViewModel.Longitude = postcodeApiModel.Result.Longitude;
        }

        _cacheService.StoreOrganisationWithService(OrganisationViewModel);

        return RedirectToPage(_cacheService.RetrieveLastPageName() == CheckServiceDetailsPageName 
            ? $"/OrganisationAdmin/{CheckServiceDetailsPageName}" 
            : "/OrganisationAdmin/WhoFor");
    }
}
