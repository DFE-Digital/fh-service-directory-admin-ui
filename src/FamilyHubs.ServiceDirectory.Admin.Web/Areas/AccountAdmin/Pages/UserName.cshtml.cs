using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;

public class UserName : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Enter a name")]
    public string? FullName { get; set; } = default!;

    [BindProperty]
    public bool ValidationValid { get; set; } = true;

    public void OnGet()
    {
        
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(FullName) || FullName.Length > 255)
        {
            ValidationValid = false;
            return Page();
        }

        return Page();
    }
}