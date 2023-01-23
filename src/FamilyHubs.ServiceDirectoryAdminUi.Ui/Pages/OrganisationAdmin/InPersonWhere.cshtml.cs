using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static FamilyHubs.ServiceDirectoryAdminUi.Ui.Infrastructure.Configuration.PageConfiguration;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

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
    private readonly IRedisCacheService _redis;

    public InPersonWhereModel(IPostcodeLocationClientService postcodeLocationClientService, ISessionService sessionService, IRedisCacheService redis)
    {
        _postcodeLocationClientService = postcodeLocationClientService;
        _redis = redis;
    }

    public void OnGet(string strOrganisationViewModel)
    {
        LastPage = _redis.RetrieveLastPageName();
        UserFlow = _redis.RetrieveUserFlow();

        OrganisationViewModel = _redis.RetrieveOrganisationWithService() ?? new OrganisationViewModel();

        OrganisationViewModel.Country = "England";

        if (!string.IsNullOrEmpty(OrganisationViewModel.Address_1))
            if (OrganisationViewModel.Address_1.Split("|").Length > 1)
            {
                Address1 = OrganisationViewModel.Address_1.Split("|")[0];
                Address2 = OrganisationViewModel.Address_1.Split("|")[1];
            }
            else
            {
                Address1 = OrganisationViewModel.Address_1;
            }
        if (!string.IsNullOrEmpty(OrganisationViewModel.City))
            City = OrganisationViewModel.City;
        if (!string.IsNullOrEmpty(OrganisationViewModel.Postal_code))
            PostalCode = OrganisationViewModel.Postal_code;
        if (!string.IsNullOrEmpty(OrganisationViewModel.State_province))
            StateProvince = OrganisationViewModel.State_province;
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
                PostalCode.Replace(" ", string.Empty).Length < 6 || PostalCode.Replace(" ", String.Empty).Length > 7)
                PostcodeValid = false;

            return Page();
        }

        if (!string.IsNullOrEmpty(PostalCode))
            InPersonSelection.Add("Our own location");

        OrganisationViewModel = _redis.RetrieveOrganisationWithService() ?? new OrganisationViewModel();
        OrganisationViewModel.InPersonSelection = new List<string>(InPersonSelection);
        OrganisationViewModel.Address_1 = Address1 + "|" + Address2;
        OrganisationViewModel.City = City;
        OrganisationViewModel.State_province = StateProvince;
        OrganisationViewModel.Country = "England";
        OrganisationViewModel.Postal_code = PostalCode;

        if (!string.IsNullOrEmpty(PostalCode))
        {
            var postcodeApiModel = await _postcodeLocationClientService.LookupPostcode(PostalCode);
            
            OrganisationViewModel.Latitude = postcodeApiModel.Result.Latitude;
            OrganisationViewModel.Longtitude = postcodeApiModel.Result.Longitude;
        }

        _redis.StoreOrganisationWithService(OrganisationViewModel);

        if (_redis.RetrieveLastPageName() == CheckServiceDetailsPageName)
        {
            return RedirectToPage($"/OrganisationAdmin/{CheckServiceDetailsPageName}");
        }
        return RedirectToPage("/OrganisationAdmin/WhoFor");

    }
}
