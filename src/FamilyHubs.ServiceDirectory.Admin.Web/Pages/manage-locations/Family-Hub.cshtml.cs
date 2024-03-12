using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Models.LocationJourney;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Razor.FullPages.Radios;
using FamilyHubs.SharedKernel.Razor.FullPages.Radios.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_locations;

[Authorize(Roles = $"{RoleTypes.DfeAdmin},{RoleGroups.LaManagerOrDualRole}")]
public class Family_HubModel : LocationPageModel, IRadiosPageModel
{
    public IEnumerable<IRadio> Radios => CommonRadios.YesNo;

    [BindProperty]
    public string? SelectedValue { get; set; }

    public string? DescriptionPartial => null;
    public string? Legend => "Is this location a family hub?";

    public Family_HubModel(IRequestDistributedCache connectionRequestCache)
        : base(LocationJourneyPage.Family_Hub, connectionRequestCache)
    {
    }

    protected override void OnGetWithModel()
    {
        SelectedValue = LocationModel!.IsFamilyHub?.ToString();
    }

    protected override IActionResult OnPostWithModel()
    {
        if (SelectedValue == null)
        {
            return RedirectToSelf(ErrorId.Family_Hub__SelectFamilyHub);
        }

        bool isFamilyHub = bool.Parse(SelectedValue);

        //todo: have HasBeenUpdated on the base??
        if (Flow == JourneyFlow.Edit)
        {
            LocationModel!.Updated = LocationModel.Updated || HasFamilyHubBeenUpdated(isFamilyHub);
        }

        LocationModel!.IsFamilyHub = isFamilyHub;

        return NextPage();
    }

    private bool HasFamilyHubBeenUpdated(bool isFamilyHub)
    {
        return LocationModel!.IsFamilyHub != isFamilyHub;
    }
}