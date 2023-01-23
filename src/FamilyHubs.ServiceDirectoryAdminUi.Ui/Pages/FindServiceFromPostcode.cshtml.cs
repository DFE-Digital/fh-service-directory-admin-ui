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

        var postcodeApiModel = await _postcodeLocationClientService.LookupPostcode(postCode.ToString());


        return RedirectToPage("LocalOfferResults", new
        {
            latitude = postcodeApiModel.Result.Latitude,
            longitude = postcodeApiModel.Result.Longitude,
            distance = 32186.9 //212892.0
        });
    }
}
