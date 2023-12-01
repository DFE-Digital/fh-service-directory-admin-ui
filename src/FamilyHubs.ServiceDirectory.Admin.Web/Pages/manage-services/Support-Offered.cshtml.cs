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
    public List<long> SelectedTaxonomies {  get; set; }
    public List<long?> SelectedCategories { get; set; }

    public Support_OfferedModel(IRequestDistributedCache connectionRequestCache, ITaxonomyService taxonomyService, IServiceDirectoryClient serviceDirectoryClient)
        : base(ServiceJourneyPage.Support_Offered, connectionRequestCache)
    {
        _taxonomyService = taxonomyService;
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    protected override async Task OnGetWithModelAsync(CancellationToken cancellationToken)
    {
        if (Errors.HasErrors)
        {
            return;
        }

        Taxonomies = await _taxonomyService.GetCategories();

        switch (Flow)
        {
            case JourneyFlow.Edit:
                if (ServiceId != null)
                {
                    var service = await _serviceDirectoryClient.GetServiceById(ServiceId!.Value, cancellationToken);
                    SelectedCategories = service.Taxonomies.Select(x => x.ParentId).Distinct().ToList();
                    SelectedTaxonomies = service.Taxonomies.Select(x=>x.Id).ToList();
                }
               
                break;

        }
    }
    protected override Task<IActionResult> OnPostWithModelAsync(CancellationToken cancellationToken)
    {

        //foreach (var key in Request.Form.Keys)
        //{
        //    if (Request.Form[key] == "true")
        //    {
        //        //selectedIds.Add(int.Parse(key));
        //    }
        //}

        return Task.FromResult(NextPage());
    }
}