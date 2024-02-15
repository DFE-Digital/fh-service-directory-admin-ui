using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Razor.FullPages.Radios;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class Add_LocationModel : ServicePageModel, IRadiosPageModel
{
    public static Radio[] StaticRadios => new[]
    {
        new Radio("Yes", true.ToString()),
        new Radio("No", false.ToString())
    };

    public IEnumerable<IRadio> Radios => StaticRadios;

    [BindProperty]
    public string? SelectedValue { get; set; }

    public string? DescriptionPartial => null;
    public string? Legend => "Do you want to add any locations for this service?";

    public Add_LocationModel(IRequestDistributedCache connectionRequestCache)
        : base(ServiceJourneyPage.Add_Location, connectionRequestCache)
    {
    }

    protected override void OnGetWithModel()
    {
        SelectedValue = ServiceModel!.AddLocations?.ToString();
    }

    protected override IActionResult OnPostWithModel()
    {
        if (SelectedValue == null)
        {
            return RedirectToSelf(ErrorId.Add_Location__MissingSelection);
        }

        bool addLocations = bool.Parse(SelectedValue);

        ServiceModel!.Updated = ServiceModel.Updated || HasAddLocationsBeenUpdated(addLocations);

        ServiceModel!.AddLocations = addLocations;

        return NextPage();
    }

    private bool HasAddLocationsBeenUpdated(bool addLocations)
    {
        return ServiceModel!.AddLocations != addLocations;
    }
}