using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.La;

[Authorize(Roles = $"{RoleTypes.LaDualRole},{RoleTypes.LaManager}")]
public class PersonsDetailsModel : PageModel
{
    [BindProperty]
    public List<string> ContactSelection { get; set; } = default!;

    [BindProperty]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    public string? Email { get; set; }

    [BindProperty]
    [Phone(ErrorMessage = "Please enter a valid phone number")]
    public string? Telephone { get; set; }

    [BindProperty]
    public string? Name { get; set; }
    [BindProperty]
    public string? Postcode { get; set; }

    [BindProperty]
    [Phone(ErrorMessage = "Please enter a valid phone number")]
    public string? Textphone { get; set; }

    [BindProperty]
    public bool ValidationValid { get; set; } = true;

    [BindProperty]
    public bool OneOptionSelected { get; set; } = true;

    [BindProperty]
    public bool EmailValid { get; set; } = true;

    [BindProperty]
    public bool PhoneValid { get; set; } = true;

    [BindProperty]
    public bool TextValid { get; set; } = true;

    [BindProperty]
    public bool NameValid { get; set; } = true;

    [BindProperty]
    public bool PostcodeValid { get; set; } = true;

    private readonly IRequestDistributedCache _requestCache;

    public PersonsDetailsModel(IRequestDistributedCache requestCache)
    {
        _requestCache = requestCache;
    }

    public async Task OnGet()
    {
        var user = HttpContext.GetFamilyHubsUser();
        SubjectAccessRequestViewModel? subjectAccessRequestViewModel = await _requestCache.GetSarAsync(user.Email);
        if (subjectAccessRequestViewModel == null)
        {
            return;
        }

        ContactSelection = new List<string> { subjectAccessRequestViewModel.SelectionType };

        switch (subjectAccessRequestViewModel.SelectionType)
        {
            case "email":
                Email = subjectAccessRequestViewModel.Value1;
                break;

            case "phone":
                Telephone = subjectAccessRequestViewModel.Value1;
                break;

            case "textphone":
                Textphone = subjectAccessRequestViewModel.Value1;
                break;

            case "nameandpostcode":
                Name = subjectAccessRequestViewModel.Value1;
                Postcode = subjectAccessRequestViewModel.Value2;
                break;
        }
    }

    public async Task<IActionResult> OnPost()
    {
        if (ContactSelection.Contains("email") && (string.IsNullOrWhiteSpace(Email) || !Regex.IsMatch(Email, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$")))
        {
            EmailValid = false;
            ValidationValid = false;
        }

        if (ContactSelection.Contains("phone") && (string.IsNullOrWhiteSpace(Telephone) || !Regex.IsMatch(Telephone, @"^[A-Za-z0-9]*$")))
        {
            PhoneValid = false;
            ValidationValid = false;
        }

        if (ContactSelection.Contains("textphone") && (string.IsNullOrWhiteSpace(Textphone) || !Regex.IsMatch(Textphone, @"^[A-Za-z0-9]*$")))
        {
            TextValid = false;
            ValidationValid = false;
        }

        if (ContactSelection.Contains("nameandpostcode"))
        {
            if (string.IsNullOrEmpty(Name))
            {
                NameValid = false;
                ValidationValid = false;
            }

            if (string.IsNullOrEmpty(Postcode))
            {
                PostcodeValid = false;
                ValidationValid = false;
            }
        }

        if (!ContactSelection.Any() && string.IsNullOrEmpty(Email) && string.IsNullOrEmpty(Telephone) && string.IsNullOrEmpty(Textphone) && string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(Postcode))
        {
            EmailValid = false;
            PhoneValid = false;
            TextValid = false;
            NameValid = false;
            PostcodeValid = false;
            ValidationValid = false;
            return Page();
        }

        if (!ModelState.IsValid || !ValidationValid)
        {
            return Page();
        }

        SubjectAccessRequestViewModel subjectAccessRequestViewModel = default!;

        switch (ContactSelection[0])
        {
            case "email":
                subjectAccessRequestViewModel = new SubjectAccessRequestViewModel
                {
                    SelectionType = ContactSelection[0],
                    Value1 = Email,
                };
                break;

            case "phone":
                subjectAccessRequestViewModel = new SubjectAccessRequestViewModel
                {
                    SelectionType = ContactSelection[0],
                    Value1 = Telephone,
                };
                break;

            case "textphone":
                subjectAccessRequestViewModel = new SubjectAccessRequestViewModel
                {
                    SelectionType = ContactSelection[0],
                    Value1 = Textphone,
                };
                break;

            case "nameandpostcode":
                subjectAccessRequestViewModel = new SubjectAccessRequestViewModel
                {
                    SelectionType = ContactSelection[0],
                    Value1 = Name,
                    Value2 = Postcode,
                };
                break;
        }

        var user = HttpContext.GetFamilyHubsUser();

        if (user == null || subjectAccessRequestViewModel == null)
        {
            return Page();
        }

        await _requestCache.SetSarAsync(user.Email, subjectAccessRequestViewModel);

        return RedirectToPage("/La/SubjectAccessResultDetails");

    }

}
