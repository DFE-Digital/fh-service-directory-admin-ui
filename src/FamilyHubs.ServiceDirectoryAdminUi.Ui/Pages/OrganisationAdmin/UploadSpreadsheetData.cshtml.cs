using System.ComponentModel.DataAnnotations;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.DataUpload;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class UploadSpreadsheetDataModel : PageModel
{
    private readonly IDataUploadService _dataUploadService;
    private readonly IRedisCacheService _redis;

    [BindProperty]
    public bool UseSpreadsheetServiceId { get; set; }

    [BindProperty]
    public string OrganisationId { get; set; } = default!;

    [BindProperty]
    public BufferedSingleFileUploadDb FileUpload { get; set; } = default!;

    public List<string> UploadErrors { get; set; } = default!;

    public bool ShowSuccess { get; set; }

    public UploadSpreadsheetDataModel(IDataUploadService dataUploadService, IRedisCacheService redis)
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
        if (FileUpload.FormFile == null)
        {
            ModelState.AddModelError("File", "File not selected.");
            return Page();
        }

        if (ModelState.IsValid)
        {
            //bool useSpreadsheetServiceId = UseSpreadsheetServiceId.Any(x => x == "UseSpreadsheetServiceId");
            //UploadErrors = await _datauploadService.UploadToApi(OrganisationId, FileUpload, UseSpreadsheetServiceId);
            UploadErrors = await _dataUploadService.UploadToApi(OrganisationId, FileUpload );

            if (UploadErrors == null || !UploadErrors.Any())
            {
                ShowSuccess = true;
            }
        }

        //using (var memoryStream = new MemoryStream())
        //{
        //    await FileUpload.FormFile.CopyToAsync(memoryStream);

        //    // Upload the file if less than 2 MB
        //    if (memoryStream.Length < 2097152)
        //    {
        //        var file = new AppFile()
        //        {
        //            Content = memoryStream.ToArray()
        //        };
        //    }
        //    else
        //    {
        //        ModelState.AddModelError("File", "The file is too large.");
        //    }
        //}

        return Page();
    }

    public IActionResult OnPostGotoHomePage()
    {
        var organisation = _redis.RetrieveOrganisationWithService();
        return RedirectToPage("/OrganisationAdmin/Welcome", new
        {
            organisationId = organisation?.Id,
        });
    }
}

public class BufferedSingleFileUploadDb
{
    [Required]
    [Display(Name = "File")]
    public IFormFile FormFile { get; set; } = default!;
}

public class AppFile
{
    public int Id { get; set; }
    public byte[] Content { get; set; } = default!;
}
