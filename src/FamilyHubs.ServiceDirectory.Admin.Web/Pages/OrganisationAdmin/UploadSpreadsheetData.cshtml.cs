using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Core.Services.DataUpload;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.OrganisationAdmin;

public class UploadSpreadsheetDataModel : PageModel
{
    private readonly IDataUploadService _dataUploadService;
    private readonly IRedisCacheService _redis;

    [BindProperty]
    public bool UseSpreadsheetServiceId { get; set; }

    [BindProperty]
    public string OrganisationId { get; set; } = null!;

    [BindProperty]
    public BufferedSingleFileUploadDb FileUpload { get; set; } = null!;

    public List<string> UploadErrors { get; set; } = new List<string>();

    public bool ShowSuccess { get; set; }

    public UploadSpreadsheetDataModel(
        IDataUploadService dataUploadService, 
        IRedisCacheService redis)
    {
        _dataUploadService = dataUploadService;
        _redis = redis;
    }

    public void OnGet(string organisationId)
    {
        OrganisationId = organisationId;
        UseSpreadsheetServiceId = true;
    }

    public async Task<IActionResult> OnPostUploadAsync()
    {
        if (ModelState.IsValid)
        {
            UploadErrors = await _dataUploadService.UploadToApi(FileUpload);

            if (!UploadErrors.Any())
            {
                ShowSuccess = true;
            }
        }

        return Page();
    }

    public IActionResult OnPostRedirectToHomePage()
    {
        var organisation = _redis.RetrieveOrganisationWithService();
        return RedirectToPage("/OrganisationAdmin/Welcome", new
        {
            organisationId = organisation?.Id
        });
    }
}