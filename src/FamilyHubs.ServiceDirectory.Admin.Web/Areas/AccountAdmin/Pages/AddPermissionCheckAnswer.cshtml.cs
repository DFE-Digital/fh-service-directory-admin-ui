using System.Text;
using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;

public class AddPermissionCheckAnswer : PageModel
{
    private readonly ICacheService _cacheService;
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
    
    public AddPermissionCheckAnswer(ICacheService cacheService, IIdamClient idamClient, IEmailService emailService)
    {
        _cacheService = cacheService;
        _idamClient = idamClient;
        _emailService = emailService;
    }
    
    public async Task OnGet()
    {
        await SetAnswerDetails();
    }

    public async Task<IActionResult> OnPost()
    {
        var permissionModel = await SetAnswerDetails();
        var dto = new AccountDto { Name = Name, Email = Email };

        dto.Claims.Add(new AccountClaimDto { Name = FamilyHubsClaimTypes.Role, Value = _role });

        var organisationId = LaJourney ? _laOrganisationId.ToString() : _vcsOrganisationId.ToString();
        dto.Claims.Add(new AccountClaimDto { Name = FamilyHubsClaimTypes.OrganisationId, Value = organisationId });

        await _emailService.SendAccountPermissionAddedEmail(permissionModel);
        
        await _idamClient.AddAccount(dto);

        return RedirectToPage("/Confirmation");
    }

    private async Task<PermissionModel> SetAnswerDetails()
    {
        var cachedModel = await _cacheService.GetPermissionModel();
        ArgumentNullException.ThrowIfNull(cachedModel);

        SetTypeOfPermission(cachedModel);

        WhoFor = cachedModel.LaJourney
            ? "Someone who works for a local authority"
            : "Someone who works for a voluntary and community sector organisation";

        VcsOrganisationName = cachedModel.VcsOrganisationName;
        
        LaOrganisationName = cachedModel.LaOrganisationName;

        Email = cachedModel.EmailAddress;

        Name = cachedModel.FullName;

        LaJourney = cachedModel.LaJourney;

        _vcsOrganisationId = cachedModel.VcsOrganisationId;
        _laOrganisationId = cachedModel.LaOrganisationId;
        _role = GetRole(cachedModel);

        return cachedModel;
    }

    private void SetTypeOfPermission(PermissionModel cachedModel)
    {
        var typeofPermission = new StringBuilder();

        if (cachedModel.LaJourney)
        {
            if (cachedModel.LaManager)
            {
                typeofPermission.Append("Add and manage services, family hubs and accounts");
            }
            
            if (cachedModel is { LaManager: true, LaProfessional: true })
            {
                typeofPermission.Append(", ");
            }
            
            if (cachedModel.LaProfessional)
            {
                typeofPermission.Append("Make connection requests to voluntary and community sector services");
            }
        }

        if (cachedModel.VcsJourney)
        {
            if (cachedModel.VcsManager)
            {
                typeofPermission.Append("Add and manage services");
            }

            if (cachedModel is { VcsManager: true, VcsProfessional: true })
            {
                typeofPermission.Append(", ");
            }

            if (cachedModel.VcsProfessional)
            {
                typeofPermission.Append("View and respond to connection requests");
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