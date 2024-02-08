using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Authorization;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using Microsoft.AspNetCore.Mvc;
using FamilyHubs.ServiceDirectory.Shared.Dto;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

[Authorize(Roles = RoleGroups.AdminRole)]
public class Service_DetailModel : ServicePageModel
{
    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public Service_DetailModel(
        IRequestDistributedCache connectionRequestCache,
        IServiceDirectoryClient serviceDirectoryClient)
        : base(ServiceJourneyPage.Service_Detail, connectionRequestCache)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    protected override async Task<IActionResult> OnPostWithModelAsync(CancellationToken cancellationToken)
    {
        if (Flow == JourneyFlow.Edit)
        {
            await UpdateService(cancellationToken);
            return RedirectToPage("/manage-services/Service-Edited-Confirmation");
        }

        await AddService(cancellationToken);
        return RedirectToPage("/manage-services/Service-Saved-Confirmation");
    }

    private Task AddService(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private async Task UpdateService(CancellationToken cancellationToken)
    {
        long serviceId = ServiceModel!.Id!.Value;
        var service = await _serviceDirectoryClient.GetServiceById(serviceId, cancellationToken);
        if (service is null)
        {
            //todo: better exception?
            throw new InvalidOperationException($"Service not found: {serviceId}");
        }

        await UpdateServiceFromCache(service, cancellationToken);

        await _serviceDirectoryClient.UpdateService(service, cancellationToken);
    }

    private async Task UpdateServiceFromCache(ServiceDto service, CancellationToken cancellationToken)
    {
        service.Name = ServiceModel!.Name!;
        service.Description = ServiceModel.Description;

        await UpdateTaxonomies(service, cancellationToken);
        UpdateServiceCost(service);
        throw new NotImplementedException();
    }

    private void UpdateServiceCost(ServiceDto service)
    {
        if (ServiceModel!.HasCost == true)
        {
            service.CostOptions = new List<CostOptionDto>
            {
                new()
                {
                    AmountDescription = ServiceModel.CostDescription
                }
            };
        }
        else
        {
            service.CostOptions = new List<CostOptionDto>();
        }
    }

    private async Task UpdateTaxonomies(ServiceDto service, CancellationToken cancellationToken)
    {
        //todo: update to accept cancellation token
        var taxonomies = await _serviceDirectoryClient.GetTaxonomyList(1, 999999);

        var selectedTaxonomies = taxonomies.Items.Where(x => ServiceModel!.SelectedSubCategories.Contains(x.Id)).ToList();

        service.Taxonomies = selectedTaxonomies;
    }
}