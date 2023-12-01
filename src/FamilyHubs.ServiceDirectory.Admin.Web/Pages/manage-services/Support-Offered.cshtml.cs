using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

[Authorize(Roles = RoleGroups.AdminRole)]
public class Support_OfferedModel : ServiceWithCachePageModel
{
    private readonly ITaxonomyService _taxonomyService;
    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public List<KeyValuePair<TaxonomyDto, List<TaxonomyDto>>> Taxonomies { get; set; }

    [BindProperty]
    public List<long?> SelectedCategories { get; set; } = new List<long?>();

    [BindProperty]
    public List<long> SelectedSubCategories { get; set; } = new List<long>();


    public Support_OfferedModel(IRequestDistributedCache connectionRequestCache, ITaxonomyService taxonomyService, IServiceDirectoryClient serviceDirectoryClient)
        : base(ServiceJourneyPage.Support_Offered, connectionRequestCache)
    {
        _taxonomyService = taxonomyService;
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    protected override async Task OnGetWithModelAsync(CancellationToken cancellationToken)
    {
        Taxonomies = await _taxonomyService.GetCategories();

        if (Errors.HasErrors)
        {
            return;
        }        

        switch (Flow)
        {
            case JourneyFlow.Edit:
                if (ServiceId != null)
                {
                    var service = await _serviceDirectoryClient.GetServiceById(ServiceId!.Value, cancellationToken);
                    SelectedCategories = service.Taxonomies.Select(x => x.ParentId).Distinct().ToList();
                    SelectedSubCategories = service.Taxonomies.Select(x => x.Id).ToList();
                }

                break;

        }
    }
    protected override Task<IActionResult> OnPostWithModelAsync(CancellationToken cancellationToken)
    {
        //no selection 
        if (SelectedCategories.Count == 0 && SelectedSubCategories.Count == 0)
        {
            return Task.FromResult(RedirectToSelf(null, ErrorId.Support_Offered__SelectCategory));
        }

        //no sub category selection 
        if ( SelectedCategories.Count > 0 && SelectedSubCategories.Count == 0 )
        {
            return Task.FromResult(RedirectToSelf(null, ErrorId.Support_Offered__SelectSubCategory));
        }

        return Task.FromResult(NextPage());
    }

    
}