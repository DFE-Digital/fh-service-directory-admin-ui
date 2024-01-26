using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Helpers;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_locations;

public class AddressUserInput
{
    public string? BuildingName { get; set; } = string.Empty;
    public string? Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; } = string.Empty;
    public string? TownOrCity { get; set; } = string.Empty;
    public string? County { get; set; } = string.Empty;    
    public string? Postcode { get; set; } = string.Empty;
}

public class Location_AddressModel : LocationPageModel<AddressUserInput>
{
    

    [BindProperty]
    public AddressUserInput UserInput { get; set; } = new();

    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public Location_AddressModel(IRequestDistributedCache connectionRequestCache, IServiceDirectoryClient serviceDirectoryClient)
        : base(LocationJourneyPage.Location_Address, connectionRequestCache)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    protected override async Task OnGetWithModelAsync(CancellationToken cancellationToken)
    {
        if (Errors.HasErrors)
        {
            UserInput = LocationModel!.UserInput!;
            return;
        }

        switch (Flow)
        {
            case JourneyFlow.Edit:
                //todo
                break;

            default:
                UserInput.BuildingName = LocationModel!.BuildingName;
                UserInput.Line1 = LocationModel!.Line1;
                UserInput.Line2 = LocationModel!.Line2;
                UserInput.TownOrCity = LocationModel!.TownOrCity;
                UserInput.County = LocationModel!.County;
                UserInput.Postcode = LocationModel!.Postcode;
                break;
        }
    }

    protected override async Task<IActionResult> OnPostWithModelAsync(CancellationToken cancellationToken)
    {
        var errors = GetAddressErrors(UserInput);
        if (errors.Any())
        {
            return RedirectToSelf(UserInput, errors.ToArray());
        }
        switch (Flow)
        {
            case JourneyFlow.Edit:
                //todo
                break;
            default:
                LocationModel!.BuildingName = UserInput.BuildingName;
                LocationModel!.Line1 = UserInput.Line1;
                LocationModel!.Line2 = UserInput.Line2;
                LocationModel!.TownOrCity = UserInput.TownOrCity;
                LocationModel!.County = UserInput.County;
                LocationModel!.Postcode = UserInput.Postcode; //todo sanitaze postcode
                break;
        }

        return NextPage();
    }

    private List<ErrorId> GetAddressErrors(AddressUserInput addressUserInput)
    {
        List<ErrorId> errors = new();

        if (string.IsNullOrWhiteSpace(addressUserInput.Line1))
        {
            errors.Add(ErrorId.Location_Address__MissingFirstLine);
        }

        if (string.IsNullOrWhiteSpace(addressUserInput.TownOrCity))
        {
            errors.Add(ErrorId.Location_Address__MissingTownOrCity);
        }

        if (string.IsNullOrWhiteSpace(addressUserInput.Postcode))
        {
            errors.Add(ErrorId.Location_Address__MissingPostcode);
        }
        else
        {
            if (!ValidationHelper.IsValidPostcode(addressUserInput.Postcode))
            {
                errors.Add(ErrorId.Location_Address__InvalidPostcode);
            }
        }
        
        return errors;
    }
}