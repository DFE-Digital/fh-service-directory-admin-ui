using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.SharedKernel.Razor.FullPages.Radios;
using FamilyHubs.SharedKernel.Razor.FullPages.Radios.Common;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class Remove_LocationModel : ServicePageModel, IRadiosPageModel
{
    private readonly IServiceDirectoryClient _serviceDirectoryClient;
    public IEnumerable<IRadio> Radios => CommonRadios.YesNo;

    [BindProperty]
    public string? SelectedValue { get; set; }

    public string? DescriptionPartial => null;
    public string? Legend { get; private set; }

    //todo: add hint
    public LocationDto? Location { get; private set; }

    public Remove_LocationModel(
        IRequestDistributedCache connectionRequestCache,
        IServiceDirectoryClient serviceDirectoryClient)
        : base(ServiceJourneyPage.Remove_Location, connectionRequestCache)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    protected override async Task OnGetWithModelAsync(CancellationToken cancellationToken)
    {
        // alternative is for link to post back and store in model
        long locationId = long.Parse(Request.Query["locationId"]);

        if (!ServiceModel!.LocationIds.Contains(locationId))
        {
            throw new InvalidOperationException("Location to remove not associated with service");
        }

        Location = await _serviceDirectoryClient.GetLocationById(locationId, cancellationToken);
        Legend = $"Do you want to remove {Location.Name ?? string.Join(", ", Location.Address1, Location.Address2)} from this service?";
    }

    protected override IActionResult OnPostWithModel()
    {
        if (SelectedValue == null)
        {
            return RedirectToSelf(ErrorId.Remove_Location__MissingSelection);
        }

        bool removeLocation = bool.Parse(SelectedValue);

        if (removeLocation)
        {
            //todo: this will remove from list of locations
            ServiceModel!.CurrentLocation = null;
        }

        return RedirectToServicePage(ServiceJourneyPage.Locations_For_Service, Flow == JourneyFlow.AddRedo ? JourneyFlow.Add : Flow);
    }
}