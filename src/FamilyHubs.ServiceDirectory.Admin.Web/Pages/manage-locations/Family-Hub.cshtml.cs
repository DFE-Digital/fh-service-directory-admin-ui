using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Razor.FullPages.Radios;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_locations;

public class Family_HubModel : LocationPageModel, IRadiosPageModel
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
    public string? Legend => "Is this location a family hub?";

    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public Family_HubModel(
        IRequestDistributedCache connectionRequestCache,
        IServiceDirectoryClient serviceDirectoryClient)
        : base(LocationJourneyPage.Family_Hub, connectionRequestCache)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    protected override async Task OnGetWithModelAsync(CancellationToken cancellationToken)
    {
        if (Errors.HasErrors)
        {
            return;
        }

        switch (Flow)
        {
            case JourneyFlow.Edit:
                var location = await _serviceDirectoryClient.GetLocationById(LocationId!.Value, cancellationToken);

                //todo:
                //SelectedValue = location.IsFamilyHub;
                break;

            default:
                SelectedValue = LocationModel!.IsFamilyHub?.ToString();
                break;
        }
    }

    protected override async Task<IActionResult> OnPostWithModelAsync(CancellationToken cancellationToken)
    {
        if (SelectedValue == null)
        {
            return RedirectToSelf(ErrorId.Family_Hub__SelectFamilyHub);
        }

        bool isFamilyHub = bool.Parse(SelectedValue);

        switch (Flow)
        {
            case JourneyFlow.Edit:
                await UpdateLocationFamilyHub(isFamilyHub, cancellationToken);
                break;
            default:
                LocationModel!.IsFamilyHub = isFamilyHub;
                break;
        }

        return NextPage();
    }

    private Task UpdateLocationFamilyHub(bool isFamilyHub, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}