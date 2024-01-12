using System.Collections.Immutable;
using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using FamilyHubs.SharedKernel.Razor.ErrorNext;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.MyAccount.Pages;

public class ChangeNameModel : HeaderPageModel
{
    public enum ErrorId
    {
        EnterAName
    }

    public static readonly ImmutableDictionary<int, PossibleError> AllErrors = ImmutableDictionary
        .Create<int, PossibleError>()
        .Add(ErrorId.EnterAName, "Enter a name");

    private readonly IIdamClient _idamClient;

    public IErrorState Errors { get; private set; }

    [BindProperty]
    public string? FullName { get; set; }

    public ChangeNameModel(IIdamClient idamClient)
    {
        _idamClient = idamClient;

        Errors = ErrorState.Empty;
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

            var request = new UpdateAccountSelfServiceDto
            {
                AccountId = long.Parse(familyHubsUser.AccountId),
                Name = FullName
            };
            await _idamClient.UpdateAccountSelfService(request, cancellationToken);

            HttpContext.RefreshClaims();

            return RedirectToPage("ChangeNameConfirmation");
        }

        Errors = ErrorState.Create(AllErrors, ErrorId.EnterAName);

        return Page();
    }
}