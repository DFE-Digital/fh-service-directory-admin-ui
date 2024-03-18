using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services;
using FamilyHubs.ServiceDirectory.Admin.Web.Errors;
using FamilyHubs.ServiceDirectory.Admin.Web.ViewModel;
using FamilyHubs.SharedKernel.Razor.ErrorNext;
using FamilyHubs.SharedKernel.Razor.FullPages.Checkboxes;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages;

public class TypeOfUserVcs : AccountAdminViewModel, ICheckboxesPageModel
{
    public IEnumerable<ICheckbox> Checkboxes => new[]
    {
        new Checkbox("Add and manage services", nameof(VcsManager)),
        new Checkbox("View and respond to connection requests", nameof(VcsProfessional))
    };

    [BindProperty]
    public IEnumerable<string> SelectedValues { get; set; } = Enumerable.Empty<string>();
    public IErrorState Errors { get; protected set; } = ErrorState.Empty;
    public string DescriptionPartial => "TypeOfUserVcsDescription";
    public string? Legend => null;
    public string? Hint => null;

    public TypeOfUserVcs(ICacheService cacheService) : base(nameof(TypeOfUserVcs), cacheService)
    {
    }

    public bool VcsProfessional => SelectedValues.Contains(nameof(VcsProfessional));
    public bool VcsManager => SelectedValues.Contains(nameof(VcsManager));

    public override async Task OnGet()
    {
        await base.OnGet();
        
        SelectedValues = new[] {PermissionModel.VcsManager ? nameof(VcsManager) : null, PermissionModel.VcsProfessional ? nameof(VcsProfessional) : null}.OfType<string>();
    }
    
    public override async Task<IActionResult> OnPost()
    {
        await base.OnPost();
        
        if (ModelState.IsValid && (VcsManager || VcsProfessional))
        {
            PermissionModel.LaManager = false;
            PermissionModel.LaProfessional = false;

            PermissionModel.VcsProfessional = VcsProfessional;
            PermissionModel.VcsManager = VcsManager;
            await CacheService.StorePermissionModel(PermissionModel, CacheId);
            
            return RedirectToPage(NextPageLink, new {cacheId = CacheId});
        }

        Errors = ErrorState.Create(PossibleErrors.All, ErrorId.AccountAdmin_TypeOfUserVcs_MissingSelection);
        return Page();
    }
}