using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.Extensions.Logging;
using Notify.Interfaces;

namespace FamilyHubs.ServiceDirectory.Admin.Core.Services;

public interface IEmailService
{
    Task SendAccountPermissionAddedEmail(PermissionModel model);
    Task SendLaPermissionChangeEmail(PermissionChangeNotificationModel notification);
    Task SendVcsPermissionChangeEmail(PermissionChangeNotificationModel notification);
}

public class EmailService : IEmailService
{
    private readonly IAsyncNotificationClient _notificationClient;
    private readonly ILogger<EmailService> _logger;

    private const string LaDualRoleEmailTemplateId = "eee5cb96-8387-4095-a942-dfe4885b4db3";
    private const string LaManagerEmailTemplateId = "cc0ba892-c9ae-4990-a07d-f38c4062fd59";
    private const string LaProfessionalEmailTemplateId = "5074a730-74bc-42fd-ad5b-d1100d7f11ca";

    private const string VcsDualRoleEmailTemplateId = "74acfbed-428e-49d6-b35c-9b6279b4b2ee";
    private const string VcsManagerEmailTemplateId = "2bfb0fdd-374f-478f-b842-973466a96efe";
    private const string VcsProfessionalEmailTemplateId = "6d5e73b8-5db8-497e-892f-6edd7e5506ec";


    public EmailService(IAsyncNotificationClient notificationClient, ILogger<EmailService> logger)
    {
        _notificationClient = notificationClient;
        _logger = logger;
    }

    public async Task SendAccountPermissionAddedEmail(PermissionModel model)
    {
        try
        {
            // TODO: Replace new Dictionary<string, dynamic>() with new keyvaluepair
            await _notificationClient.SendEmailAsync(model.EmailAddress, GetEmailTemplateId(model), new Dictionary<string, dynamic>());

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
            return LaDualRoleEmailTemplateId;
        }

        if (model.LaManager)
        {
            return LaManagerEmailTemplateId;
        }

        if (model.LaProfessional)
        {
            return LaProfessionalEmailTemplateId;
        }

        if (model.VcsDualRole)
        {
            return VcsDualRoleEmailTemplateId;
        }

        if (model.VcsManager)
        {
            return VcsManagerEmailTemplateId;
        }

        if (model.VcsProfessional)
        {
            return VcsProfessionalEmailTemplateId;
        }

        throw new InvalidOperationException("Valid role not find in account permission view model, unable to send confirmation email");
    }


    public async Task SendLaPermissionChangeEmail(PermissionChangeNotificationModel notification)
    {
        try
        {
            var templateId = GetLaPermissionChangeTemplateId(notification.OldRole, notification.NewRole);
            // TODO: Replace new Dictionary<string, dynamic>() with new keyvaluepair
            await _notificationClient.SendEmailAsync(notification.EmailAddress, templateId, new Dictionary<string, dynamic>());

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
            return "5fd7a0e7-1126-4c1d-9626-026689ec1c7e";
        }
        else if (oldRole == RoleTypes.LaProfessional && newRole == RoleTypes.LaManager)
        {
            return "8533816f-0cd3-4ecb-9725-f41fd526ab73";
        }
        else if (oldRole == RoleTypes.LaManager && newRole == RoleTypes.LaDualRole)
        {
            return "b79fe749-b99e-4b4a-9bfc-797f662c20cb";
        }
        else if (oldRole == RoleTypes.LaProfessional && newRole == RoleTypes.LaDualRole)
        {
            return "684eaef7-d234-4e87-b41c-c6764cd5f01a";
        }
        else if (oldRole == RoleTypes.LaDualRole && newRole == RoleTypes.LaManager)
        {
            return "0ee9cafc-2d8d-4649-bd34-179188e67ad4";
        }
        else if (oldRole == RoleTypes.LaDualRole && newRole == RoleTypes.LaProfessional)
        {
            return "f3f1ec29-3048-4f37-b285-630d69d931c0";
        }

        throw new InvalidOperationException("Valid email template not found in Permission Change Notification Model, unable to send confirmation email for LA");
    }

    public async Task SendVcsPermissionChangeEmail(PermissionChangeNotificationModel notification)
    {
        try
        {
            var templateId = GetVcsPermissionChangeTemplateId(notification.OldRole, notification.NewRole);
            // TODO: Replace new Dictionary<string, dynamic>() with new keyvaluepair
            await _notificationClient.SendEmailAsync(notification.EmailAddress, templateId, new Dictionary<string, dynamic>());

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
            return "d8b02428-3bd8-477a-a052-8d522ec6126f";
        }
        else if (oldRole == RoleTypes.VcsProfessional && newRole == RoleTypes.VcsManager)
        {
            return "a9dbc333-dfbb-475c-b89f-bd446f8d9724";
        }
        else if (oldRole == RoleTypes.VcsManager && newRole == RoleTypes.VcsDualRole)
        {
            return "7c6e253d-8492-4c06-9164-dc244375d961";
        }
        else if (oldRole == RoleTypes.VcsProfessional && newRole == RoleTypes.VcsDualRole)
        {
            return "b39e9655-bfce-42b2-8f5e-09f73c372282";
        }
        else if (oldRole == RoleTypes.VcsDualRole && newRole == RoleTypes.VcsManager)
        {
            return "e7349937-8d55-43d8-8d3c-3ebbcd3652d5";
        }
        else if (oldRole == RoleTypes.VcsDualRole && newRole == RoleTypes.VcsProfessional)
        {
            return "ffbd4db9-3b9a-4d99-9c7e-a53b68e754f2";
        }

        throw new InvalidOperationException("Valid email template not found in Permission Change Notification Model, unable to send confirmation email for VCS");
    }
}