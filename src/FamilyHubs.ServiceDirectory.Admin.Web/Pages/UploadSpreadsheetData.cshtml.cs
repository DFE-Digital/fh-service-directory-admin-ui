using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services.DataUpload;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages;

[Authorize(Roles = "DfeAdmin")]
public class UploadSpreadsheetDataModel : HeaderPageModel
{
    private readonly IDataUploadService _dataUploadService;

    [BindProperty]
    public BufferedSingleFileUploadDb FileUpload { get; set; } = null!;

    public List<string> UploadErrors { get; set; } = new();

    public bool ShowSuccess { get; set; }

    public UploadSpreadsheetDataModel(IDataUploadService dataUploadService)
    {
        _dataUploadService = dataUploadService;
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
}