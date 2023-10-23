using System.Collections.Immutable;
using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Razor.Errors;
using Microsoft.AspNetCore.Mvc;
using ErrorDictionary = System.Collections.Immutable.ImmutableDictionary<int, FamilyHubs.SharedKernel.Razor.Errors.Error>;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.MyAccount.Pages;

public class ChangeNameModel : HeaderPageModel
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
    public string? FullName { get; set; }

    public ChangeNameModel(IIdamClient idamClient)
    {
        _idamClient = idamClient;

        ErrorState = SharedKernel.Razor.Errors.ErrorState.Empty;
    }

    public void OnGet()
    {
        FullName = HttpContext.GetFamilyHubsUser().FullName;
    }

    //todo: PRG?
    public async Task<IActionResult> OnPost(CancellationToken cancellationToken)
    {
        if (ModelState.IsValid && !string.IsNullOrWhiteSpace(FullName) && FullName.Length <= 255)
        {
            var familyHubsUser = HttpContext.GetFamilyHubsUser();

            var request = new UpdateAccountSelfServiceDto {
                AccountId = long.Parse(familyHubsUser.AccountId),
                Name = FullName
                //Email = familyHubsUser.Email
            };
            await _idamClient.UpdateAccountSelfService(request, cancellationToken);

            HttpContext.RefreshClaims();

            return RedirectToPage("ChangeNameConfirmation");
        }

        //todo: overload/replace with params version
        ErrorState = SharedKernel.Razor.Errors.ErrorState.Create(AllErrors, new[] {ErrorId.EnterAName});

        return Page();
    }
}