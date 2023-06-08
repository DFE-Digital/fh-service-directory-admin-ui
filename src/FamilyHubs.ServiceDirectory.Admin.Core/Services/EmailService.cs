using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using Microsoft.Extensions.Logging;
using Notify.Interfaces;
using Notify.Models.Responses;

namespace FamilyHubs.ServiceDirectory.Admin.Core.Services;

public interface IEmailService
{
    Task<bool> SendAccountPermissionAddedEmail(PermissionModel model);
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

    public async Task<bool> SendAccountPermissionAddedEmail(PermissionModel model)
    {
        try
        {
            await _notificationClient.SendEmailAsync(model.EmailAddress, GetEmailTemplateId(model), new Dictionary<string, dynamic>());
            
            _logger.LogInformation("Account Permission Added Email Sent");

            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error sending Account Permission Added Email");
            return false;
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
}