using FamilyHubs.Notification.Api.Client;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.Extensions.Logging;

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
    private readonly ILogger<EmailService> _logger;

    public EmailService(INotifications notificationClient, ILogger<EmailService> logger)
    {
        _notificationClient = notificationClient;
        _logger = logger;
    }

    public async Task SendAccountPermissionAddedEmail(PermissionModel model)
    {
        try
        {
            // TODO: Replace new Dictionary<string, dynamic>() with new keyvaluepair
            await _notificationClient.SendEmailsAsync(new List<string>() { model.EmailAddress }, GetEmailTemplateId(model), new Dictionary<string, string>(), Notification.Api.Contracts.ApiKeyType.ManageKey);

            _logger.LogInformation("Account Permission Added Email Sent");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error sending Account Permission Added Email");
            throw;
        }
    }

    private string GetEmailTemplateId(PermissionModel model)
    {
        if (model.LaDualRole)
        {
            return EmailTempaltes.LaPermissionAddedDualRoleEmailTemplateId;
        }

        if (model.LaManager)
        {
            return EmailTempaltes.LaPermissionAddedManagerEmailTemplateId;
        }

        if (model.LaProfessional)
        {
            return EmailTempaltes.LaPermissionAddedProfessionalEmailTemplateId;
        }

        if (model.VcsDualRole)
        {
            return EmailTempaltes.VcsPermissionAddedDualRoleEmailTemplateId;
        }

        if (model.VcsManager)
        {
            return EmailTempaltes.VcsPermissionAddedManagerEmailTemplateId;
        }

        if (model.VcsProfessional)
        {
            return EmailTempaltes.VcsPermissionAddedProfessionalEmailTemplateId;
        }

        throw new InvalidOperationException("Valid role not find in account permission view model, unable to send confirmation email");
    }


    public async Task SendLaPermissionChangeEmail(PermissionChangeNotificationModel notification)
    {
        try
        {
            var templateId = GetLaPermissionChangeTemplateId(notification.OldRole, notification.NewRole);
            // TODO: Replace new Dictionary<string, dynamic>() with new keyvaluepair
            await _notificationClient.SendEmailsAsync(new List<string>() { notification.EmailAddress }, templateId, new Dictionary<string, string>(), Notification.Api.Contracts.ApiKeyType.ManageKey);

            _logger.LogInformation("Account Permission Modified Email template {templateId} Sent", templateId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error sending Account Permission Modified Email LA");
            throw;
        }
    }    

    private string GetLaPermissionChangeTemplateId(string oldRole, string newRole)
    {
        if (oldRole == RoleTypes.LaManager && newRole == RoleTypes.LaProfessional)
        {
            return EmailTempaltes.LaPermissionChangedFromLaManagerToLaProfessional;
        }
        
        if (oldRole == RoleTypes.LaProfessional && newRole == RoleTypes.LaManager)
        {
            return EmailTempaltes.LaPermissionChangedFromLaProfessionalToLaManager;
        }
        
        if (oldRole == RoleTypes.LaManager && newRole == RoleTypes.LaDualRole)
        {
            return EmailTempaltes.LaPermissionChangedFromLaManagerToLaDualRole;
        }
        
        if (oldRole == RoleTypes.LaProfessional && newRole == RoleTypes.LaDualRole)
        {
            return EmailTempaltes.LaPermissionChangedFromLaProfessionalToLaDualRole;
        }
        
        if (oldRole == RoleTypes.LaDualRole && newRole == RoleTypes.LaManager)
        {
            return EmailTempaltes.LaPermissionChangedFromLaDualRoleToLaManager;
        }
        
        if (oldRole == RoleTypes.LaDualRole && newRole == RoleTypes.LaProfessional)
        {
            return EmailTempaltes.LaPermissionChangedFromLaDualRoleToLaProfessional;
        }

        throw new InvalidOperationException("Valid email template not found in Permission Change Notification Model, unable to send confirmation email for LA");
    }

    public async Task SendVcsPermissionChangeEmail(PermissionChangeNotificationModel notification)
    {
        try
        {
            var templateId = GetVcsPermissionChangeTemplateId(notification.OldRole, notification.NewRole);
            // TODO: Replace new Dictionary<string, dynamic>() with new keyvaluepair
            await _notificationClient.SendEmailsAsync(new List<string>() { notification.EmailAddress }, templateId, new Dictionary<string, string>(), Notification.Api.Contracts.ApiKeyType.ManageKey);

            _logger.LogInformation("Account Permission Modified Email template {templateId} Sent", templateId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error sending Account Permission Modified Email VCS");
            throw;
        }
    }

    private string GetVcsPermissionChangeTemplateId(string oldRole, string newRole)
    {
        if (oldRole == RoleTypes.VcsManager && newRole == RoleTypes.VcsProfessional)
        {
            return EmailTempaltes.VcsPermissionChangedFromVcsManagerToVcsProfessional;
        }
        
        if (oldRole == RoleTypes.VcsProfessional && newRole == RoleTypes.VcsManager)
        {
            return EmailTempaltes.VcsPermissionChangedFromVcsProfessionalToVcsManager;
        }
        
        if (oldRole == RoleTypes.VcsManager && newRole == RoleTypes.VcsDualRole)
        {
            return EmailTempaltes.VcsPermissionChangedFromVcsManagerToVcsDualRole;
        }
        
        if (oldRole == RoleTypes.VcsProfessional && newRole == RoleTypes.VcsDualRole)
        {
            return EmailTempaltes.VcsPermissionChangedFromVcsProfessionalToVcsDualRole;
        }
        
        if (oldRole == RoleTypes.VcsDualRole && newRole == RoleTypes.VcsManager)
        {
            return EmailTempaltes.VcsPermissionChangedFromVcsDualRoleToVcsManager;
        }
        
        if (oldRole == RoleTypes.VcsDualRole && newRole == RoleTypes.VcsProfessional)
        {
            return EmailTempaltes.VcsPermissionChangedFromVcsDualRoleToVcsProfessional;
        }

        throw new InvalidOperationException("Valid email template not found in Permission Change Notification Model, unable to send confirmation email for VCS");
    }

    public async Task SendAccountEmailUpdatedEmail(EmailChangeNotificationModel model)
    {
        try
        {
            var templateId = GetEmailUpdatedTemplateId(model.Role);
            // TODO: Replace new Dictionary<string, dynamic>() with new keyvaluepair
            // Use UIBaseUrl from appsettings
            var tokens = new Dictionary<string, string>()
            {
                { "LaManageConnectionRequestsStartPage", "https://www.connect-families-to-support.education.gov.uk/" },
                { "LaManageFamilySupportServicesAndAccountsStartPage", "value2" } 
            };

            

            await _notificationClient.SendEmailsAsync(new List<string>() { model.EmailAddress }, templateId, tokens, Notification.Api.Contracts.ApiKeyType.ManageKey);

            _logger.LogInformation("Account Email Updated Email Sent");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error sending Account Email Updated Email");
            throw;
        }
    }

    private string GetEmailUpdatedTemplateId(string role)
    {
        if (role == RoleTypes.LaDualRole)
        {
            return EmailTempaltes.LaEmailUpdatedForLaDualRole;
        }

        if (role == RoleTypes.LaManager)
        {
            return EmailTempaltes.LaEmailUpdatedForLaManager;
        }

        if (role == RoleTypes.LaProfessional)
        {
            return EmailTempaltes.LaEmailUpdatedForLaProfessional;
        }

        if (role == RoleTypes.VcsDualRole)
        {
            return EmailTempaltes.VcsEmailUpdatedForVcsDualRole;
        }

        if (role == RoleTypes.VcsManager)
        {
            return EmailTempaltes.VcsEmailUpdatedForVcsManager;
        }

        if (role == RoleTypes.VcsProfessional)
        {
            return EmailTempaltes.VcsEmailUpdatedForVcsProfessional;
        }

        throw new InvalidOperationException("Valid role not found, unable to send confirmation email for email change");
    }

    public async Task SendAccountDeletedEmail(AccountDeletedNotificationModel model)
    {
        try
        {
            var templateId = GetAccountDeletedTemplateId(model.Role);
            // TODO: Replace new Dictionary<string, dynamic>() with new keyvaluepair
            await _notificationClient.SendEmailsAsync(new List<string>() { model.EmailAddress }, templateId, new Dictionary<string, string>(), Notification.Api.Contracts.ApiKeyType.ManageKey);

            _logger.LogInformation("Account Deleted Confirmation Email Sent");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error sending Account Deleted Confirmation Email");
            throw;
        }
    }

    private string GetAccountDeletedTemplateId(string role)
    {
        if (role == RoleTypes.LaDualRole)
        {
            return EmailTempaltes.LaAccountDeletedForLaDualRole;
        }

        if (role == RoleTypes.LaManager)
        {
            return EmailTempaltes.LaAccountDeletedForLaManager;
        }

        if (role == RoleTypes.LaProfessional)
        {
            return EmailTempaltes.LaAccountDeletedForLaProfessional;
        }

        if (role == RoleTypes.VcsDualRole)
        {
            return EmailTempaltes.VcsAccountDeletedForVcsDualRole;
        }

        if (role == RoleTypes.VcsManager)
        {
            return EmailTempaltes.VcsAccountDeletedForVcsManager;
        }

        if (role == RoleTypes.VcsProfessional)
        {
            return EmailTempaltes.VcsAccountDeletedForVcsProfessional;
        }

        throw new InvalidOperationException("Valid role not found, unable to send confirmation email for accound deletion.");
    }
}

