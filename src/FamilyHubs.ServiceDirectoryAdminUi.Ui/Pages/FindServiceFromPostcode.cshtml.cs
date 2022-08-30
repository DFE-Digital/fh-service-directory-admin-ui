using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages;

public class FindServiceFromPostcodeModel : PageModel
{
    private readonly IPostcodeLocationClientService _postcodeLocationClientService;
    public string Postcode { get; set; } = default!;

    public FindServiceFromPostcodeModel(IPostcodeLocationClientService postcodeLocationClientService)
    {
        _postcodeLocationClientService = postcodeLocationClientService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost()
    {
        var postCode = Request.Form["Postcode"];

        if (string.IsNullOrEmpty(postCode))
        {
            return new RedirectToPageResult("/FindServiceFromPostcode");
        }

        PostcodeApiModel postcodeApiModel = await _postcodeLocationClientService.LookupPostcode(postCode);


        return RedirectToPage("LocalOfferResults", new
        {
            postcodeApiModel.result.latitude,
            postcodeApiModel.result.longitude,
            distance = 32186.9 //212892.0
        });

        //return new RedirectToPageResult($"/LocalOfferResults/{lat}/{lng}");
    }
}
