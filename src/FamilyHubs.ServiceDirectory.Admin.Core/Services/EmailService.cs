using FamilyHubs.Notification.Api.Client;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Razor.FamilyHubsUi.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FamilyHubs.ServiceDirectory.Admin.Core.Services;

public interface IEmailService
{
    Task SendAccountPermissionAddedEmail(PermissionModel model);
    Task SendLaPermissionChangeEmail(PermissionChangeNotificationModel notification);
    Task SendVcsPermissionChangeEmail(PermissionChangeNotificationModel notification);
    Task SendAccountEmailUpdatedEmail(EmailChangeNotificationModel model);
    Task SendAccountDeletedEmail(AccountDeletedNotificationModel model);
}

public class EmailService : IEmailService
{
    private readonly INotifications _notificationClient;
    private readonly FamilyHubsUiOptions _familyHubsUiOptions;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<FamilyHubsUiOptions> configuration, INotifications notificationClient, ILogger<EmailService> logger)
    {
        _familyHubsUiOptions = configuration.Value;
        _notificationClient = notificationClient;
        _logger = logger;
    }

    public async Task SendAccountPermissionAddedEmail(PermissionModel model)
    {
        var tokens = new Dictionary<string, string>()
        {                
            { "ConnectFamiliesToSupportStartPage", _familyHubsUiOptions.Url(UrlKeys.ConnectWeb).ToString()  },
            { "ManageFamilySupportServicesStartPage", _familyHubsUiOptions.Url(UrlKeys.ManageWeb).ToString() }
        };

        await _notificationClient.SendEmailsAsync(new List<string>() { model.EmailAddress }, GetEmailTemplateId(model), tokens, Notification.Api.Contracts.ApiKeyType.ManageKey);

        _logger.LogInformation("Account Permission Added Email Sent");
    }

    private string GetEmailTemplateId(PermissionModel model)
    {
        if (model.LaDualRole)
        {
            return EmailTemplates.LaPermissionAddedDualRoleEmailTemplateId;
        }

        if (model.LaManager)
        {
            return EmailTemplates.LaPermissionAddedManagerEmailTemplateId;
        }

        if (model.LaProfessional)
        {
            return EmailTemplates.LaPermissionAddedProfessionalEmailTemplateId;
        }

        if (model.VcsDualRole)
        {
            return EmailTemplates.VcsPermissionAddedDualRoleEmailTemplateId;
        }

        if (model.VcsManager)
        {
            return EmailTemplates.VcsPermissionAddedManagerEmailTemplateId;
        }

        if (model.VcsProfessional)
        {
            return EmailTemplates.VcsPermissionAddedProfessionalEmailTemplateId;
        }

        throw new InvalidOperationException("Valid role not find in account permission view model, unable to send confirmation email");
    }


    public async Task SendLaPermissionChangeEmail(PermissionChangeNotificationModel notification)
    {
        var templateId = GetLaPermissionChangeTemplateId(notification.OldRole, notification.NewRole);

        var tokens = new Dictionary<string, string>()
        {                
            { "ConnectFamiliesToSupportStartPage", _familyHubsUiOptions.Url(UrlKeys.ConnectWeb).ToString()  },
            { "ManageFamilySupportServicesStartPage", _familyHubsUiOptions.Url(UrlKeys.ManageWeb).ToString() }
        };
        
        await _notificationClient.SendEmailsAsync(new List<string>() { notification.EmailAddress }, templateId, tokens, Notification.Api.Contracts.ApiKeyType.ManageKey);

        _logger.LogInformation("Account Permission Modified Email template {TemplateId} Sent", templateId);
    }    

    private string GetLaPermissionChangeTemplateId(string oldRole, string newRole)
    {
        if (oldRole == RoleTypes.LaManager && newRole == RoleTypes.LaProfessional)
        {
            return EmailTemplates.LaPermissionChangedFromLaManagerToLaProfessional;
        }
        
        if (oldRole == RoleTypes.LaProfessional && newRole == RoleTypes.LaManager)
        {
            return EmailTemplates.LaPermissionChangedFromLaProfessionalToLaManager;
        }
        
        if (oldRole == RoleTypes.LaManager && newRole == RoleTypes.LaDualRole)
        {
            return EmailTemplates.LaPermissionChangedFromLaManagerToLaDualRole;
        }
        
        if (oldRole == RoleTypes.LaProfessional && newRole == RoleTypes.LaDualRole)
        {
            return EmailTemplates.LaPermissionChangedFromLaProfessionalToLaDualRole;
        }
        
        if (oldRole == RoleTypes.LaDualRole && newRole == RoleTypes.LaManager)
        {
            return EmailTemplates.LaPermissionChangedFromLaDualRoleToLaManager;
        }
        
        if (oldRole == RoleTypes.LaDualRole && newRole == RoleTypes.LaProfessional)
        {
            return EmailTemplates.LaPermissionChangedFromLaDualRoleToLaProfessional;
        }

        throw new InvalidOperationException("Valid email template not found in Permission Change Notification Model, unable to send confirmation email for LA");
    }

    public async Task SendVcsPermissionChangeEmail(PermissionChangeNotificationModel notification)
    {
        var templateId = GetVcsPermissionChangeTemplateId(notification.OldRole, notification.NewRole);

        var tokens = new Dictionary<string, string>()
        {                
            { "ConnectFamiliesToSupportStartPage", _familyHubsUiOptions.Url(UrlKeys.ConnectWeb).ToString()  },
            { "ManageFamilySupportServicesStartPage", _familyHubsUiOptions.Url(UrlKeys.ManageWeb).ToString() }
        };

        await _notificationClient.SendEmailsAsync(new List<string>() { notification.EmailAddress }, templateId, tokens, Notification.Api.Contracts.ApiKeyType.ManageKey);

        _logger.LogInformation("Account Permission Modified Email template {TemplateId} Sent", templateId);
    }

    private string GetVcsPermissionChangeTemplateId(string oldRole, string newRole)
    {
        if (oldRole == RoleTypes.VcsManager && newRole == RoleTypes.VcsProfessional)
        {
            return EmailTemplates.VcsPermissionChangedFromVcsManagerToVcsProfessional;
        }
        
        if (oldRole == RoleTypes.VcsProfessional && newRole == RoleTypes.VcsManager)
        {
            return EmailTemplates.VcsPermissionChangedFromVcsProfessionalToVcsManager;
        }
        
        if (oldRole == RoleTypes.VcsManager && newRole == RoleTypes.VcsDualRole)
        {
            return EmailTemplates.VcsPermissionChangedFromVcsManagerToVcsDualRole;
        }
        
        if (oldRole == RoleTypes.VcsProfessional && newRole == RoleTypes.VcsDualRole)
        {
            return EmailTemplates.VcsPermissionChangedFromVcsProfessionalToVcsDualRole;
        }
        
        if (oldRole == RoleTypes.VcsDualRole && newRole == RoleTypes.VcsManager)
        {
            return EmailTemplates.VcsPermissionChangedFromVcsDualRoleToVcsManager;
        }
        
        if (oldRole == RoleTypes.VcsDualRole && newRole == RoleTypes.VcsProfessional)
        {
            return EmailTemplates.VcsPermissionChangedFromVcsDualRoleToVcsProfessional;
        }

        throw new InvalidOperationException("Valid email template not found in Permission Change Notification Model, unable to send confirmation email for VCS");
    }

    public async Task SendAccountEmailUpdatedEmail(EmailChangeNotificationModel model)
    {
        var templateId = GetEmailUpdatedTemplateId(model.Role);
        
        var tokens = new Dictionary<string, string>()
        {                
            { "ConnectFamiliesToSupportStartPage", _familyHubsUiOptions.Url(UrlKeys.ConnectWeb).ToString()  },
            { "ManageFamilySupportServicesStartPage", _familyHubsUiOptions.Url(UrlKeys.ManageWeb).ToString() } 
        };            

        await _notificationClient.SendEmailsAsync(new List<string>() { model.EmailAddress }, templateId, tokens, Notification.Api.Contracts.ApiKeyType.ManageKey);

        _logger.LogInformation("Account Email Updated Email Sent");
    }

    private string GetEmailUpdatedTemplateId(string role)
    {
        if (role == RoleTypes.LaDualRole)
        {
            return EmailTemplates.LaEmailUpdatedForLaDualRole;
        }

        if (role == RoleTypes.LaManager)
        {
            return EmailTemplates.LaEmailUpdatedForLaManager;
        }

        if (role == RoleTypes.LaProfessional)
        {
            return EmailTemplates.LaEmailUpdatedForLaProfessional;
        }

        if (role == RoleTypes.VcsDualRole)
        {
            return EmailTemplates.VcsEmailUpdatedForVcsDualRole;
        }

        if (role == RoleTypes.VcsManager)
        {
            return EmailTemplates.VcsEmailUpdatedForVcsManager;
        }

        if (role == RoleTypes.VcsProfessional)
        {
            return EmailTemplates.VcsEmailUpdatedForVcsProfessional;
        }

        throw new InvalidOperationException("Valid role not found, unable to send confirmation email for email change");
    }

    public async Task SendAccountDeletedEmail(AccountDeletedNotificationModel model)
    {
        var templateId = GetAccountDeletedTemplateId(model.Role);

        var tokens = new Dictionary<string, string>()
        {                
            { "ConnectFamiliesToSupportStartPage", _familyHubsUiOptions.Url(UrlKeys.ConnectWeb).ToString()  },
            { "ManageFamilySupportServicesStartPage", _familyHubsUiOptions.Url(UrlKeys.ManageWeb).ToString() }
        };

        await _notificationClient.SendEmailsAsync(new List<string>() { model.EmailAddress }, templateId, tokens, Notification.Api.Contracts.ApiKeyType.ManageKey);

        _logger.LogInformation("Account Deleted Confirmation Email Sent");
    }

    private string GetAccountDeletedTemplateId(string role)
    {
        if (role == RoleTypes.LaDualRole)
        {
            return EmailTemplates.LaAccountDeletedForLaDualRole;
        }

        if (role == RoleTypes.LaManager)
        {
            return EmailTemplates.LaAccountDeletedForLaManager;
        }

        if (role == RoleTypes.LaProfessional)
        {
            return EmailTemplates.LaAccountDeletedForLaProfessional;
        }

        if (role == RoleTypes.VcsDualRole)
        {
            return EmailTemplates.VcsAccountDeletedForVcsDualRole;
        }

        if (role == RoleTypes.VcsManager)
        {
            return EmailTemplates.VcsAccountDeletedForVcsManager;
        }

        if (role == RoleTypes.VcsProfessional)
        {
            return EmailTemplates.VcsAccountDeletedForVcsProfessional;
        }

        throw new InvalidOperationException("Valid role not found, unable to send confirmation email for accound deletion.");
    }
}

