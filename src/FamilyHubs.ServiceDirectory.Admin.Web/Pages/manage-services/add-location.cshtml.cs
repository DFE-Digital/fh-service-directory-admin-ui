using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Models.ServiceJourney;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Razor.FullPages.Radios;
using FamilyHubs.SharedKernel.Razor.FullPages.Radios.Common;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class Add_LocationModel : ServicePageModel, IRadiosPageModel
{
    public IEnumerable<IRadio> Radios => CommonRadios.YesNo;

    [BindProperty]
    public string? SelectedValue { get; set; }

    public string? DescriptionPartial => "add-location-content";
    public string? Legend => null;

    public Add_LocationModel(IRequestDistributedCache connectionRequestCache)
        : base(ServiceJourneyPage.Add_Location, connectionRequestCache)
    {
    }

    protected override void OnGetWithModel()
    {
        SelectedValue = ServiceModel!.AddingLocations?.ToString();
    }

    protected override IActionResult OnPostWithModel()
    {
        if (SelectedValue == null)
        {
            return RedirectToSelf(ErrorId.Add_Location__MissingSelection);
        }

        bool addLocations = bool.Parse(SelectedValue);

        //todo: just selecting yes or no doesn't indicate that something's changes
        // the act of actually adding a location is what changes the service
        //ServiceModel!.Updated = ServiceModel.Updated || HasAddLocationsBeenUpdated(addLocations);

        ServiceModel!.AddingLocations = addLocations;

        if (!addLocations)
        {
            ServiceModel!.CurrentLocation = null;
        }

        return NextPage();
    }

    //private bool HasAddLocationsBeenUpdated(bool addLocations)
    //{
    //    return ServiceModel!.AddLocations != addLocations;
    //}
}