using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class Locations_For_ServiceModel : ServicePageModel
{
    public List<LocationDto> Locations { get; private set; }

    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public Locations_For_ServiceModel(
        IRequestDistributedCache connectionRequestCache,
        IServiceDirectoryClient serviceDirectoryClient)
        : base(ServiceJourneyPage.Locations_For_Service, connectionRequestCache)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    protected override async Task OnGetWithModelAsync(CancellationToken cancellationToken)
    {
        Locations = await GetLocations(_serviceDirectoryClient, cancellationToken);
    }

    protected override IActionResult OnPostWithModel()
    {
        return NextPage();
    }
}