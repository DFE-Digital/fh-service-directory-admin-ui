using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using static FamilyHubs.ServiceDirectoryAdminUi.Ui.Infrastructure.Configuration.PageConfiguration;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class InPersonWhereModel : PageModel
{
    public string LastPage { get; set; } = default!;
    public string UserFlow { get; set; } = default!;

    [BindProperty]
    public string Address_1 { get; set; } = default!;
    [BindProperty]
    public string Address_2 { get; set; } = default!;
    [BindProperty]
    public string City { get; set; } = default!;
    [BindProperty]
    public string Postal_code { get; set; } = default!;
    [BindProperty]
    public string Country { get; set; } = default!;
    [BindProperty]
    public string State_province { get; set; } = default!;

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
    public bool PostcodeAPIValid { get; set; } = true;

    private readonly IPostcodeLocationClientService _postcodeLocationClientService;
    private readonly ISessionService _session;
    private readonly IRedisCacheService _redis;

    public InPersonWhereModel(IPostcodeLocationClientService postcodeLocationClientService, ISessionService sessionService, IRedisCacheService redis)
    {
        _postcodeLocationClientService = postcodeLocationClientService;
        _session = sessionService;
        _redis = redis;
    }

    public void OnGet(string strOrganisationViewModel)
    {
        LastPage = _redis.RetrieveLastPageName();
        UserFlow = _redis.RetrieveUserFlow();

        OrganisationViewModel = _redis.RetrieveOrganisationWithService() ?? new OrganisationViewModel();

        OrganisationViewModel.Country = "England";

        if (OrganisationViewModel != null)
        {
            if (!string.IsNullOrEmpty(OrganisationViewModel.Address_1))
                if (OrganisationViewModel.Address_1.Split("|").Length > 1)
                {
                    Address_1 = OrganisationViewModel.Address_1.Split("|")[0];
                    Address_2 = OrganisationViewModel.Address_1.Split("|")[1];
                }
                else
                {
                    Address_1 = OrganisationViewModel.Address_1;
                }
            if (!string.IsNullOrEmpty(OrganisationViewModel.City))
                City = OrganisationViewModel.City;
            if (!string.IsNullOrEmpty(OrganisationViewModel.Postal_code))
                Postal_code = OrganisationViewModel.Postal_code;
            if (!string.IsNullOrEmpty(OrganisationViewModel.State_province))
                State_province = OrganisationViewModel.State_province;
            if (OrganisationViewModel.InPersonSelection != null && OrganisationViewModel.InPersonSelection.Any())
                InPersonSelection = OrganisationViewModel.InPersonSelection;
        }

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
            PostcodeApiModel postcodeApiModel = await _postcodeLocationClientService.LookupPostcode(Postal_code);
        }
        catch
        {
            PostcodeAPIValid = false;
        }

        if (!ModelState.IsValid ||
            string.IsNullOrEmpty(Address_1) ||
            string.IsNullOrEmpty(City) ||
            string.IsNullOrEmpty(Postal_code) ||
            PostcodeAPIValid == false)
        {
            ValidationValid = false;

            if (string.IsNullOrEmpty(Address_1))
                Address1Valid = false;

            if (string.IsNullOrEmpty(City))
                TownCityValid = false;

            if (string.IsNullOrEmpty(Postal_code) || PostcodeAPIValid == false ||
                Postal_code.Replace(" ", String.Empty).Length < 6 || Postal_code.Replace(" ", String.Empty).Length > 7)
                PostcodeValid = false;

            return Page();
        }

        //TODO - why we need this?
        if (!string.IsNullOrEmpty(Postal_code))
            InPersonSelection.Add("Our own location");


        OrganisationViewModel = _redis.RetrieveOrganisationWithService() ?? new OrganisationViewModel();
        OrganisationViewModel.InPersonSelection = new List<string>(InPersonSelection);
        OrganisationViewModel.Address_1 = Address_1 + "|" + Address_2;
        OrganisationViewModel.City = City;
        OrganisationViewModel.State_province = State_province;
        OrganisationViewModel.Country = "England";
        OrganisationViewModel.Postal_code = Postal_code;

        if (!string.IsNullOrEmpty(Postal_code))
        {
            PostcodeApiModel postcodeApiModel = await _postcodeLocationClientService.LookupPostcode(Postal_code);
            if (postcodeApiModel != null)
            {
                OrganisationViewModel.Latitude = postcodeApiModel.result.latitude;
                OrganisationViewModel.Longtitude = postcodeApiModel.result.longitude;
            }
        }

        _redis.StoreOrganisationWithService(OrganisationViewModel);

        if (_redis.RetrieveLastPageName() == CheckServiceDetailsPageName)
        {
            return RedirectToPage($"/OrganisationAdmin/{CheckServiceDetailsPageName}");
        }
        return RedirectToPage("/OrganisationAdmin/OfferAtFamiliesPlace");

    }
}
