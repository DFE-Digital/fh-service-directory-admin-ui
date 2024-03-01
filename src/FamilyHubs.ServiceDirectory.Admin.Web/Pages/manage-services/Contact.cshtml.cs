using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Helpers;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class ContactUserInput
{
    public string? Email { get; set; }
    public bool HasEmail { get; set; }
    public string? TelephoneNumber { get; set; }
    public bool HasTelephone { get; set; }
    public string? Website { get; set; }
    public bool HasWebsite { get; set; }
    public string? TextTelephoneNumber { get; set; }
    public bool HasTextMessage { get; set; }
}

public class ContactModel : ServicePageModel<ContactUserInput>
{
    public string TextBoxLabel { get; set; } = "How can people find out more about this service?";
    public int? EmailMaxLength => 254;
    public int? WebsiteMaxLength => 2083;
    public int? TelephoneMaxLength => 50;

    public string HintText { get; set; }

    [BindProperty]
    public ContactUserInput UserInput { get; set; } = new();

    public ContactModel(IRequestDistributedCache connectionRequestCache)
        : base(ServiceJourneyPage.Contact, connectionRequestCache)
    {
    }

    protected override void OnGetWithError()
    {
        UserInput = ServiceModel!.UserInput!;
    }

    protected override void OnGetWithModel()
    {
        if (ServiceModel!.HasEmail)
        {
            UserInput.HasEmail = true;
            UserInput.Email = ServiceModel.Email;
        }
        if (ServiceModel!.HasTelephone)
        {
            UserInput.HasTelephone = true;
            UserInput.TelephoneNumber = ServiceModel.TelephoneNumber;
        }
        if (ServiceModel!.HasWebsite)
        {
            UserInput.HasWebsite = true;
            UserInput.Website = ServiceModel.Website;
        }
        if (ServiceModel!.HasTextMessage)
        {
            UserInput.HasTextMessage = true;
            UserInput.TextTelephoneNumber = ServiceModel.TextTelephoneNumber;
        }
    }

    protected override IActionResult OnPostWithModel()
    {
        if (!UserInput.HasEmail && !UserInput.HasTelephone && !UserInput.HasWebsite && !UserInput.HasTextMessage)
        {
            return RedirectToSelf(UserInput, ErrorId.Contact__MissingSelection);
        }

        if (UserInput.HasEmail && (string.IsNullOrWhiteSpace(UserInput.Email) || !ValidationHelper.IsValidEmail(UserInput.Email)))
        {
            return RedirectToSelf(UserInput, ErrorId.Contact__MissingEmailOrIncorrectFormat);
        }

        if (UserInput.HasTelephone && string.IsNullOrWhiteSpace(UserInput.TelephoneNumber))
        {
            return RedirectToSelf(UserInput, ErrorId.Contact__MissingTelephone);
        }

        if (UserInput.HasTelephone && !ValidationHelper.IsValidPhoneNumber(UserInput.TelephoneNumber))
        {
            return RedirectToSelf(UserInput, ErrorId.Contact__TelephoneIncorrectFormat);
        }

        if (UserInput.HasWebsite && (string.IsNullOrWhiteSpace(UserInput.Website) || !ValidationHelper.IsValidUrl(UserInput.Website)))
        {
            return RedirectToSelf(UserInput, ErrorId.Contact__MissingOrInvalidWebsite);
        }

        if (UserInput.HasTextMessage && string.IsNullOrWhiteSpace(UserInput.TextTelephoneNumber))
        {
            return RedirectToSelf(UserInput, ErrorId.Contact__MissingTextMessageNumber);
        }

        if (UserInput.HasTextMessage && !ValidationHelper.IsValidPhoneNumber(UserInput.TextTelephoneNumber))
        {
            return RedirectToSelf(UserInput, ErrorId.Contact__TextMessageNumberIncorrectFormat);
        }       

        string? newEmail;
        if (UserInput.HasEmail)
        {
            ServiceModel!.HasEmail = true;
            newEmail = UserInput.Email;
        }
        else
        {
            ServiceModel!.HasEmail = false;
            newEmail = null;
        }

        string? newTelephone;
        if (UserInput.HasTelephone)
        {
            ServiceModel!.HasTelephone = true;
            newTelephone = UserInput.TelephoneNumber;
        }
        else
        {
            ServiceModel!.HasTelephone = false;
            newTelephone = null;
        }

        string? newWebsite;
        if (UserInput.HasWebsite)
        {
            ServiceModel!.HasWebsite = true;
            newWebsite = UserInput.Website;
        }
        else
        {
            ServiceModel!.HasWebsite = false;
            newWebsite = null;
        }

        string? newTextTelephoneNumber;
        if (UserInput.HasTextMessage)
        {
            ServiceModel!.HasTextMessage = true;
            newTextTelephoneNumber = UserInput.TextTelephoneNumber;
        }
        else
        {
            ServiceModel!.HasTextMessage = false;
            newTextTelephoneNumber = null;
        }

        ServiceModel!.Updated = ServiceModel.Updated
            || ServiceModel.Email != newEmail
            || ServiceModel.TelephoneNumber != newTelephone
            || ServiceModel.Website != newWebsite
            || ServiceModel.TextTelephoneNumber != newTextTelephoneNumber;

        ServiceModel.Email = newEmail;
        ServiceModel.TelephoneNumber = newTelephone;
        ServiceModel.Website = newWebsite;
        ServiceModel.TextTelephoneNumber = newTextTelephoneNumber;

        return NextPage();
    }
}