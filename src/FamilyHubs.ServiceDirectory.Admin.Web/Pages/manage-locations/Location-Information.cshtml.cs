using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Razor.FullPages.SingleTextArea;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_locations;

[Authorize(Roles = RoleGroups.AdminRole)]
public class Location_InformationModel : LocationPageModel<string?>, ISingleTextAreaPageModel
{
    [BindProperty]
    public string? TextAreaValue { get; set; }
    public string DescriptionPartial => "Location-Information-Content";
    public string? Label => null;
    public int TextAreaMaxLength => 500;
    public int TextAreaNumberOfRows => 9;

    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public Location_InformationModel(
        IRequestDistributedCache connectionRequestCache,
        IServiceDirectoryClient serviceDirectoryClient)
        : base(LocationJourneyPage.Location_Information, connectionRequestCache)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    protected override async Task OnGetWithModelAsync(CancellationToken cancellationToken)
    {
        if (Errors.HasErrors)
        {
            TextAreaValue = LocationModel!.UserInput;
            return;
        }

        switch (Flow)
        {
            case JourneyFlow.Edit:
                var location = await _serviceDirectoryClient.GetLocationById(LocationId!.Value, cancellationToken);

                TextAreaValue = location.Description;
                break;

            default:
                TextAreaValue = LocationModel!.Description;
                break;
        }
    }

    protected override async Task<IActionResult> OnPostWithModelAsync(CancellationToken cancellationToken)
    {
        var errorId = this.CheckForErrors(
            //todo: temp until change CheckForErrors to take nullable enum
            ErrorId.Service_Cost__DescriptionTooLong,
            ErrorId.Location_Information__TooLong);

        //todo: temp until change CheckForErrors to take nullable enum
        //if (errorId != null)
        if (errorId == ErrorId.Location_Information__TooLong)
        {
            //todo: need to truncate the user input to something sensible
            return RedirectToSelf(TextAreaValue, errorId.Value);
        }

        switch (Flow)
        {
            case JourneyFlow.Edit:
                await UpdateLocationDescription(TextAreaValue!, cancellationToken);
                break;
            default:
                LocationModel!.Description = TextAreaValue;
                break;
        }

        return NextPage();
    }

    private async Task UpdateLocationDescription(string serviceDescription, CancellationToken cancellationToken)
    {
        var location = await _serviceDirectoryClient.GetLocationById(LocationId!.Value, cancellationToken);
        location.Description = serviceDescription;
        await _serviceDirectoryClient.UpdateLocation(location, cancellationToken);
    }
}