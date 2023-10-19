using System.Collections.Immutable;
using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Razor.Errors;
using Microsoft.AspNetCore.Mvc;
using ErrorDictionary = System.Collections.Immutable.ImmutableDictionary<int, FamilyHubs.SharedKernel.Razor.Errors.Error>;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.MyAccount.Pages;

public class ChangeNameModel : MyAccountViewModel
{
    public enum ErrorId
    {
        EnterAName
    }

    public static readonly ErrorDictionary AllErrors = ImmutableDictionary
        .Create<int, Error>()
        .Add(ErrorId.EnterAName, "new-name", "Enter a name");

    private readonly IIdamClient _idamClient;

    public IErrorState ErrorState { get; private set; }

    [BindProperty]
    public string FullName { get; set; } = string.Empty;        

    public ChangeNameModel(IIdamClient idamClient)
    {
        PreviousPageLink = "/ViewPersonalDetails";
        HasBackButton = true;
        _idamClient = idamClient;

        ErrorState = SharedKernel.Razor.Errors.ErrorState.Empty;
    }

    public void OnGet()
    {
        var familyHubsUser = HttpContext.GetFamilyHubsUser();
        FullName = familyHubsUser.FullName;
    }

    public async Task<IActionResult> OnPost()
    {
        if (ModelState.IsValid && !string.IsNullOrWhiteSpace(FullName) && FullName.Length <= 255)
        {
            var familyHubsUser = HttpContext.GetFamilyHubsUser();

            var request = new UpdateAccountDto {
                AccountId = long.Parse(familyHubsUser.AccountId),
                Name = FullName,
                Email = familyHubsUser.Email
            };
            await _idamClient.UpdateAccount(request);

            HttpContext.RefreshClaims();

            return RedirectToPage("ChangeNameConfirmation");
        }

        //todo: overload/replace with params version
        ErrorState = SharedKernel.Razor.Errors.ErrorState.Create(AllErrors, new[] {ErrorId.EnterAName});

        return Page();
    }
}