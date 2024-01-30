using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Helpers;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

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

[Authorize(Roles = RoleGroups.AdminRole)]
public class Location_AddressModel : LocationPageModel<AddressUserInput>
{
    [BindProperty]
    public AddressUserInput UserInput { get; set; } = new();

    public Location_AddressModel(IRequestDistributedCache connectionRequestCache)
        : base(LocationJourneyPage.Location_Address, connectionRequestCache)
    {
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
                LocationModel!.Postcode = SanitisePostcode(UserInput.Postcode!);
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

    private string SanitisePostcode(string postcode)
    {
        Regex GdsAllowableCharsRegex = new(@"[-\(\)\.\[\]]+", RegexOptions.Compiled);
        Regex MultipleSpacesRegex = new(@"\s+");
        string partSanitisedPostcode = GdsAllowableCharsRegex.Replace(postcode.Trim().ToUpper(), "");
        return MultipleSpacesRegex.Replace(partSanitisedPostcode, " ");
    }
}