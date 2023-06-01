using System.Text;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;

public class AddPermissionCheckAnswer : PageModel
{
    private readonly ICacheService _cacheService;

    public string WhoFor { get; set; } = string.Empty;
    public string TypeOfPermission { get; set; } = string.Empty;
    public string LaOrganisationName { get; set; } = string.Empty;
    public string VcsOrganisationName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool LaJourney { get; set; }	
    
    public AddPermissionCheckAnswer(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }
    
    public async Task OnGet()
    {
       await SetAnswerDetails();
    }

    public async Task OnPost()
    {
       await SetAnswerDetails();
    }

    private async Task SetAnswerDetails()
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
}