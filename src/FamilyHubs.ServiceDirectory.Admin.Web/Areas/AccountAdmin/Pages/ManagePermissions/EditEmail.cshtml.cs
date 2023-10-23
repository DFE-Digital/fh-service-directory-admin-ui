using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Helpers;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages.ManagePermissions;

public class EditEmailModel : InputPageViewModel
{
    private readonly IIdamClient _idamClient;
    private readonly IEmailService _emailService;
    private readonly ILogger<EditEmailModel> _logger;

    [BindProperty(SupportsGet = true)]
    public string AccountId { get; set; } = string.Empty; //Route Property

    [BindProperty]
    public required string EmailAddress { get; set; } = string.Empty;

    public EditEmailModel(
        IIdamClient idamClient,
        IEmailService emailService,
        ILogger<EditEmailModel> logger)
    {
        PageHeading = "What's their email address?";
        ErrorMessage = "Enter an email address";
        BackButtonPath = $"/AccountAdmin/ManagePermissions/{AccountId}";
        SubmitButtonText = "Confirm";
        HintText = "They will use this to sign in to their account.";

        _idamClient = idamClient;
        _emailService = emailService;
        _logger = logger;
    }

    public void OnGet()
    {
        BackButtonPath = $"/AccountAdmin/ManagePermissions/{AccountId}";
        // being called to throw an exception if account id isn't a long
        GetAccountId();
    }

    public async Task<IActionResult> OnPost()
    {
        if (ModelState.IsValid && ValidationHelper.IsValidEmail(EmailAddress))
        {
            long accountId = GetAccountId();

            var updateDto = new UpdateAccountDto 
            { 
                AccountId = accountId,
                Email = EmailAddress,
            };

            await _idamClient.UpdateAccount(updateDto);

            try
            {
                await SendNotificationEmail(accountId);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Account email updated, but unable to send notification email");
            }

            return RedirectToPage("EditEmailChangedConfirmation", new { AccountId });
        }

        BackButtonPath = $"/AccountAdmin/ManagePermissions/{AccountId}";
        HasValidationError = true;
        return Page();
    }

    private async Task SendNotificationEmail(long accountId)
    {
        var account = await _idamClient.GetAccountById(accountId);
        if (account == null)
        {
            throw new Exception("User Account not found");
        }

        string accountRole = GetRole(account);

        var email = new EmailChangeNotificationModel
        {
            EmailAddress = EmailAddress,
            Role = accountRole
        };
        await _emailService.SendAccountEmailUpdatedEmail(email);
    }

    private long GetAccountId()
    {
        if (long.TryParse(AccountId, out long id))
        {
            return id;
        }

        throw new Exception("Invalid AccountId");
    }

    private string GetRole(AccountDto? account)
    {
        if (account is not null)
        {
            var roleClaim = account.Claims.Single(x => x.Name == FamilyHubsClaimTypes.Role);
            var role = roleClaim.Value;
            return role;
        }

        throw new Exception("Role not found");
    }
}