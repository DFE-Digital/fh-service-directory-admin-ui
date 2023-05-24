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
    public string LocalAuthority { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
	
    public AddPermissionCheckAnswer(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }
    
    public void OnGet()
    {
        SetAnswerDetails();
    }

    public void OnPost()
    {
        SetAnswerDetails();
    }

    private void SetAnswerDetails()
    {
        var cachedModel = _cacheService.GetPermissionModel();
        ArgumentNullException.ThrowIfNull(cachedModel);

        SetTypeOfPermission(cachedModel);

        WhoFor = cachedModel.LaJourney
            ? "Someone who works for a local authority"
            : "Someone who works for a voluntary and community sector organisation";

        LocalAuthority = cachedModel.OrganisationName;

        Email = cachedModel.EmailAddress;

        Name = cachedModel.FullName;
    }

    private void SetTypeOfPermission(PermissionModel cachedModel)
    {
        var typeofPermission = new StringBuilder();

        if (cachedModel.LaJourney)
        {
            if (cachedModel.LaAdmin)
            {
                typeofPermission.Append("Add and manage services, family hubs and accounts");
            }

            if (cachedModel.LaProfessional)
            {
                typeofPermission.Append("Make connection requests to voluntary and community sector services");
            }
        }

        if (cachedModel.VcsJourney)
        {
            if (cachedModel.VcsAdmin)
            {
                typeofPermission.Append("Add and manage services");
            }

            if (cachedModel.VcsAdmin && cachedModel.VcsProfessional)
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