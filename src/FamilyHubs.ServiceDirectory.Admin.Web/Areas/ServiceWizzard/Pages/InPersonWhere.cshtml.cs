using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.ServiceWizzard.Pages;

public class InPersonWhereModel : BasePageModel
{
    private readonly IPostcodeLocationClientService _postcodeLocationClientService;

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
    public bool ValidationValid { get; set; } = true;

    [BindProperty]
    public bool Address1Valid { get; set; } = true;

    [BindProperty]
    public bool TownCityValid { get; set; } = true;

    [BindProperty]
    public bool PostcodeValid { get; set; } = true;

    [BindProperty]
    public bool PostcodeApiValid { get; set; } = true;

    public InPersonWhereModel(IRequestDistributedCache requestCache, IPostcodeLocationClientService postcodeLocationClientService)
        : base(requestCache)
    {
        _postcodeLocationClientService = postcodeLocationClientService;
    }
    public async Task OnGet()
    {
        OrganisationViewModel? viewModel = await GetOrganisationViewModel();
        if (viewModel == null)
        {
            return;
        }

        viewModel.Country = "England";

        if (!string.IsNullOrEmpty(viewModel.Address1))
            if (viewModel.Address1.Split("|").Length > 1)
            {
                Address1 = viewModel.Address1.Split("|")[0];
                Address2 = viewModel.Address1.Split("|")[1];
            }
            else
            {
                Address1 = viewModel.Address1;
            }
        if (!string.IsNullOrEmpty(viewModel.City))
            City = viewModel.City;
        if (!string.IsNullOrEmpty(viewModel.PostalCode))
            PostalCode = viewModel.PostalCode;
        if (!string.IsNullOrEmpty(viewModel.StateProvince))
            StateProvince = viewModel.StateProvince;
        if (viewModel.InPersonSelection != null && viewModel.InPersonSelection.Any())
            InPersonSelection = viewModel.InPersonSelection;
    }

    public async Task<IActionResult> OnPost()
    {
        ModelState.Remove("Country");
        ModelState.Remove("Address2");
        ModelState.Remove("StateProvince");
        ModelState.Remove("PostcodeApiValid");
        Country = "England";

        try
        {
            await _postcodeLocationClientService.LookupPostcode(PostalCode);
        }
        catch
        {
            PostcodeApiValid = false;
        }

        if (!ModelState.IsValid ||
            string.IsNullOrEmpty(Address1) ||
            string.IsNullOrEmpty(City) ||
            string.IsNullOrEmpty(PostalCode) ||
            !PostcodeApiValid)
        {
            ValidationValid = false;

            if (string.IsNullOrEmpty(Address1))
                Address1Valid = false;

            if (string.IsNullOrEmpty(City))
                TownCityValid = false;

            if (string.IsNullOrEmpty(PostalCode) || !PostcodeApiValid ||
                PostalCode.Replace(" ", string.Empty).Length < 6 || PostalCode.Replace(" ", String.Empty).Length > 7)
                PostcodeValid = false;

            return Page();
        }

        if (!string.IsNullOrEmpty(PostalCode))
            InPersonSelection.Add("Our own location");

        OrganisationViewModel? viewModel = await GetOrganisationViewModel();
        if (viewModel == null)
        {
            viewModel = new OrganisationViewModel();
        }
        viewModel.InPersonSelection = new List<string>(InPersonSelection);
        viewModel.Address1 = Address1 + "|" + Address2;
        viewModel.City = City;
        viewModel.StateProvince = StateProvince;
        viewModel.Country = "England";
        viewModel.PostalCode = PostalCode;

        if (!string.IsNullOrEmpty(PostalCode))
        {
            var postcodeApiModel = await _postcodeLocationClientService.LookupPostcode(PostalCode);

            viewModel.Latitude = postcodeApiModel.Result.Latitude;
            viewModel.Longitude = postcodeApiModel.Result.Longitude;
        }

        await SetCacheAsync(viewModel);

        if (string.Compare(await GetLastPage(), "/CheckServiceDetails", StringComparison.OrdinalIgnoreCase) == 0)
        {
            return RedirectToPage("CheckServiceDetails", new { area = "ServiceWizzard" });
        }

        return RedirectToPage("ContactDetails", new { area = "ServiceWizzard" });

    }
}
