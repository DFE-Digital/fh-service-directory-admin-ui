using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Helpers;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Services.Postcode.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System.Threading;
using FamilyHubs.SharedKernel.Services.Postcode.Model;

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

    private readonly IPostcodeLookup _postcodeLookup;

    public Location_AddressModel(
        IRequestDistributedCache connectionRequestCache,
        IPostcodeLookup postcodeLookup)
        : base(LocationJourneyPage.Location_Address, connectionRequestCache)
    {
        _postcodeLookup = postcodeLookup;
    }

    protected override void OnGetWithModel()
    {
        if (Errors.HasErrors)
        {
            UserInput = LocationModel!.UserInput!;
            return;
        }

        UserInput.BuildingName = LocationModel!.Name;
        UserInput.Line1 = LocationModel!.AddressLine1;
        UserInput.Line2 = LocationModel!.AddressLine2;
        UserInput.TownOrCity = LocationModel!.City;
        UserInput.County = LocationModel!.County;
        UserInput.Postcode = LocationModel!.Postcode;
    }

    protected override async Task<IActionResult> OnPostWithModelAsync(CancellationToken cancellationToken)
    {
        IPostcodeInfo? postcodeInfo = default;
        var errors = GetAddressErrors(UserInput);
        if (!errors.Any())
        {
            (var postcodeError, postcodeInfo) = await _postcodeLookup.Get(UserInput.Postcode, cancellationToken);
            AddPostcodeErrors(postcodeError, errors);
        }
        if (errors.Any())
        {
            return RedirectToSelf(UserInput, errors.ToArray());
        }

        LocationModel!.Name = UserInput.BuildingName;
        LocationModel.AddressLine1 = UserInput.Line1;
        LocationModel.AddressLine2 = UserInput.Line2;
        LocationModel.City = UserInput.TownOrCity;
        LocationModel.County = UserInput.County;
        LocationModel.Postcode = SanitisePostcode(UserInput.Postcode!);
        LocationModel.Latitude = postcodeInfo!.Latitude;
        LocationModel.Longitude = postcodeInfo.Latitude;

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

        return errors;
    }

    private void AddPostcodeErrors(PostcodeError postcodeError, List<ErrorId> errors)
    {
        switch (postcodeError)
        {
            case PostcodeError.NoPostcode:
                errors.Add(ErrorId.Location_Address__MissingPostcode);
                break;
            case PostcodeError.InvalidPostcode:
                errors.Add(ErrorId.Location_Address__InvalidPostcode);
                break;
            case PostcodeError.PostcodeNotFound:
                errors.Add(ErrorId.Location_Address__InvalidPostcode);
                break;
        }
    }

    private string SanitisePostcode(string postcode)
    {
        Regex GdsAllowableCharsRegex = new(@"[-\(\)\.\[\]]+", RegexOptions.Compiled);
        Regex MultipleSpacesRegex = new(@"\s+");
        string partSanitisedPostcode = GdsAllowableCharsRegex.Replace(postcode.Trim().ToUpper(), "");
        return MultipleSpacesRegex.Replace(partSanitisedPostcode, " ");
    }
}