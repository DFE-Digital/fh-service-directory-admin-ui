using System.Text;
using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Constants;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;

public class AddPermissionCheckAnswer : AccountAdminViewModel
{
    private readonly IIdamClient _idamClient;
    private readonly IEmailService _emailService;
    private long _vcsOrganisationId;
    private long _laOrganisationId;
    private string _role = string.Empty;

    public string WhoFor { get; set; } = string.Empty;
    public string TypeOfPermission { get; set; } = string.Empty;
    public string LaOrganisationName { get; set; } = string.Empty;
    public string VcsOrganisationName { get; set; } = string.Empty;
    public bool IsUserLaManager => HttpContext.IsUserLaManager();
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool LaJourney { get; set; }	  
    
    public AddPermissionCheckAnswer(ICacheService cacheService, IIdamClient idamClient, IEmailService emailService) : base(nameof(AddPermissionCheckAnswer), cacheService)
    {
        _idamClient = idamClient;
        _emailService = emailService;
    }
    
    public override async Task OnGet()
    {
        await base.OnGet();
        
        SetAnswerDetails();
    }

    public override async Task<IActionResult> OnPost()
    {
        await base.OnPost();
        
        SetAnswerDetails();
        
        var dto = new AccountDto { Name = Name, Email = Email };

        dto.Claims.Add(new AccountClaimDto { Name = FamilyHubsClaimTypes.Role, Value = _role });

        var organisationId = LaJourney ? _laOrganisationId.ToString() : _vcsOrganisationId.ToString();
        dto.Claims.Add(new AccountClaimDto { Name = FamilyHubsClaimTypes.OrganisationId, Value = organisationId });

        await _emailService.SendAccountPermissionAddedEmail(PermissionModel);
        
        await _idamClient.AddAccount(dto);

        return RedirectToPage(NextPageLink, new {cacheId= CacheId });
    }

    private void SetAnswerDetails()
    {
        SetTypeOfPermission(PermissionModel);

        SetRoleTypeLabelsForCurrentUser(PermissionModel.LaOrganisationName);
        
        WhoFor = PermissionModel.LaJourney ? LaRoleTypeLabel : VcsRoleTypeLabel;

        VcsOrganisationName = PermissionModel.VcsOrganisationName;
        
        LaOrganisationName = PermissionModel.LaOrganisationName;

        Email = PermissionModel.EmailAddress;

        Name = PermissionModel.FullName;

        LaJourney = PermissionModel.LaJourney;

        _vcsOrganisationId = PermissionModel.VcsOrganisationId;
        _laOrganisationId = PermissionModel.LaOrganisationId;
        
        _role = GetRole(PermissionModel);
    }

    private void SetTypeOfPermission(PermissionModel cachedModel)
    {
        var typeofPermission = new StringBuilder();

        if (cachedModel.LaJourney)
        {
            if (cachedModel.LaManager)
            {
                typeofPermission.Append(RoleDescription.LaManager);
            }
            
            if (cachedModel is { LaManager: true, LaProfessional: true })
            {
                typeofPermission.Append(", ");
            }
            
            if (cachedModel.LaProfessional)
            {
                typeofPermission.Append(RoleDescription.LaProfessional);
            }
        }

        if (cachedModel.VcsJourney)
        {
            if (cachedModel.VcsManager)
            {
                typeofPermission.Append(RoleDescription.VcsManager);
            }

            if (cachedModel is { VcsManager: true, VcsProfessional: true })
            {
                typeofPermission.Append(", ");
            }

            if (cachedModel.VcsProfessional)
            {
                typeofPermission.Append(RoleDescription.VcsProfessional);
            }
        }

        TypeOfPermission = typeofPermission.ToString();
    }

    private string GetRole(PermissionModel cachedModel)
    {
        if (cachedModel.LaJourney)
        {
            if (cachedModel is { LaManager: true, LaProfessional: true })
            {
                return RoleTypes.LaDualRole;
            }

            if (cachedModel.LaManager)
            {
                return RoleTypes.LaManager;
            }

            if (cachedModel.LaProfessional)
            {
                return RoleTypes.LaProfessional;
            }
        }

        if (cachedModel.VcsJourney)
        {
            if (cachedModel is { VcsManager: true, VcsProfessional: true })
            {
                return RoleTypes.VcsDualRole;
            }

            if (cachedModel.VcsManager)
            {
                return RoleTypes.VcsManager;
            }

            if (cachedModel.VcsProfessional)
            {
                return RoleTypes.VcsProfessional;
            }
        }

        throw new Exception("PermissionModel has invalid role settings");
    }
}