using System.Globalization;
using System.Text.RegularExpressions;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;

public class UserEmail : AccountAdminViewModel
{
    private readonly ICacheService _cacheService;

    public UserEmail(ICacheService cacheService)
    {
        _cacheService = cacheService;
        PageHeading = "What's their email address?";
        ErrorMessage = "Enter an email address";
        BackLink = "/Welcome";
    }

    [BindProperty] 
    public required string EmailAddress { get; set; } = string.Empty;

    public async Task OnGet()
    {
        var permissionModel = await _cacheService.GetPermissionModel();
        ArgumentNullException.ThrowIfNull(permissionModel);
        
        EmailAddress = permissionModel.EmailAddress;
        BackLink = permissionModel.VcsJourney ? "/WhichVcsOrganisation" : HttpContext.IsUserLaManager() ? "/TypeOfUserLa" : "/WhichLocalAuthority";
    }
    
    public async Task<IActionResult> OnPost()
    {
        var permissionModel = await _cacheService.GetPermissionModel();
        ArgumentNullException.ThrowIfNull(permissionModel);
        
        if (ModelState.IsValid && IsValidEmail(EmailAddress))
        {
            permissionModel.EmailAddress = EmailAddress;
            await _cacheService.StorePermissionModel(permissionModel);
            
            return RedirectToPage("/UserName");
        }
        
        BackLink = permissionModel.VcsJourney ? "/WhichVcsOrganisation" : HttpContext.IsUserLaManager() ? "/TypeOfUserLa" : "/WhichLocalAuthority";
        HasValidationError = true;
        return Page();
    }

    //https://learn.microsoft.com/en-us/dotnet/standard/base-types/how-to-verify-that-strings-are-in-valid-email-format
    public static bool IsValidEmail(string email)
    {
        if (email.Length >= 255 && string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // Normalize the domain
            email = Regex.Replace(email, @"(@)(.+)$", DomainMapper, RegexOptions.None, TimeSpan.FromMilliseconds(200));

            // Examines the domain part of the email and normalizes it.
            string DomainMapper(Match match)
            {
                // Use IdnMapping class to convert Unicode domain names.
                // Pull out and process domain name (throws ArgumentException on invalid)
                var domainName = new IdnMapping().GetAscii(match.Groups[2].Value);

                return match.Groups[1].Value + domainName;
            }
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }

        try
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }
}