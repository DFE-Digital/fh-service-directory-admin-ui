using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

public class BufferedSingleFileUploadDb
{
    [Required]
    [Display(Name = "File")]
    public IFormFile FormFile { get; init; } = default!;
}