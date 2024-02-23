using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

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
        //todo: common with service detail. move somewhere common
        //todo: this will end up with a foreach
        Locations = new List<LocationDto>();
        if (ServiceModel!.CurrentLocation != null)
        {
            Locations.Add(await _serviceDirectoryClient.GetLocationById(ServiceModel.CurrentLocation.Value, cancellationToken));
        }
    }

    protected override IActionResult OnPostWithModel()
    {
        return NextPage();
    }
}