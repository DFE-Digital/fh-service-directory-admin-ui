using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Errors;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Razor.ErrorNext;
using FamilyHubs.SharedKernel.Razor.FullPages.Checkboxes;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;

public class TypeOfUserLa : AccountAdminViewModel, ICheckboxesPageModel
{
    public IEnumerable<ICheckbox> Checkboxes => new[]
    {
        new Checkbox("Add and manage services, family hubs and accounts", nameof(LaManager)),
        new Checkbox("Make connection requests to voluntary and community sector services", nameof(LaProfessional))
    };
    [BindProperty]
    public IEnumerable<string> SelectedValues { get; set; } = Enumerable.Empty<string>();
    public IErrorState Errors { get; protected set; } = ErrorState.Empty;
    public string? DescriptionPartial => null;
    public string Legend => "What do they need to do?";
    public string? Hint => null;

    public TypeOfUserLa(ICacheService cacheService) : base(nameof(TypeOfUserLa), cacheService)
    {
    }
    
    public bool LaProfessional => SelectedValues.Contains(nameof(LaProfessional));
    public bool LaManager => SelectedValues.Contains(nameof(LaManager));

    public override async Task OnGet()
    {
        await base.OnGet();

        SelectedValues = new[] {PermissionModel.LaManager ? nameof(LaManager) : null, PermissionModel.LaProfessional ? nameof(LaProfessional) : null}.OfType<string>();
    }
    
    public override async Task<IActionResult> OnPost()
    {
        await base.OnPost();
        
        if (ModelState.IsValid && (LaManager || LaProfessional))
        {
            PermissionModel.LaManager = LaManager;
            PermissionModel.LaProfessional = LaProfessional;
            
            PermissionModel.VcsManager = false;
            PermissionModel.VcsProfessional = false;
            
            PermissionModel.LaOrganisationId = HttpContext.IsUserLaManager() ? HttpContext.GetUserOrganisationId() : 0;
            
            await CacheService.StorePermissionModel(PermissionModel, CacheId);

            return RedirectToPage(NextPageLink, new {cacheId= CacheId});
        }
        
        Errors = ErrorState.Create(PossibleErrors.All, ErrorId.AccountAdmin_TypeOfUserLa_MissingSelection);
        return Page();
    }
}
