using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Dataupload;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

[Authorize(Policy = "ServiceMaintainer")]
public class UploadSpreadsheetDataModel : PageModel
{
    private readonly IDatauploadService _datauploadService;
    private readonly IRedisCacheService _redis;

    [BindProperty]
    public string OrganisationId { get; set; } = default!;

    [BindProperty]
    public BufferedSingleFileUploadDb FileUpload { get; set; } = default!;

    public List<string> UploadErrors { get; set; } = default!;

    public bool ShowSuccess { get; set; } = false;

    public UploadSpreadsheetDataModel(IDatauploadService datauploadService, IRedisCacheService redis)
    {
        _datauploadService = datauploadService;
        _redis = redis;
    }

    public void OnGet(string organisationId)
    {
        OrganisationId = organisationId;
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
            UploadErrors = await _datauploadService.UploadToApi(OrganisationId, FileUpload);

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

        //        //_dbContext.File.Add(file);

        //        //await _dbContext.SaveChangesAsync();
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
