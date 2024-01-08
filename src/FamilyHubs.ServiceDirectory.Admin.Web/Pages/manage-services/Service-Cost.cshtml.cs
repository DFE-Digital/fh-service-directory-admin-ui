using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class ServiceCostUserInput
{
    public bool? HasCost { get; set; }
    public string? Description { get; set; }
}


[Authorize(Roles = RoleGroups.AdminRole)]
public class Service_CostModel : ServicePageModel<ServiceCostUserInput>
{
    public string TextBoxLabel { get; set; } = "Does the service cost money to use?";
    public int? MaxLength => 150;

    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    [BindProperty]
    public ServiceCostUserInput UserInput { get; set; } = new();

    public Service_CostModel(
        IRequestDistributedCache connectionRequestCache,
        IServiceDirectoryClient serviceDirectoryClient)
    : base(ServiceJourneyPage.Service_Cost, connectionRequestCache)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    protected override async Task OnGetWithModelAsync(CancellationToken cancellationToken)
    {
        if (Errors.HasErrors)
        {
            UserInput = ServiceModel!.UserInput!;
            return;
        }

        switch (Flow)
        {
            case JourneyFlow.Edit:
                var service = await _serviceDirectoryClient.GetServiceById(ServiceId!.Value, cancellationToken);

                if (service.CostOptions.Count > 0)
                {
                    UserInput.HasCost = true;
                    UserInput.Description = service.CostOptions.First().AmountDescription!;
                }
                else
                {
                    UserInput.HasCost = false;
                }
                break;

            default:
                if (ServiceModel!.HasCost.HasValue && ServiceModel!.HasCost!.Value)
                {
                    UserInput.HasCost = true;
                    UserInput.Description = ServiceModel!.CostDescription!;
                }
                else if (ServiceModel!.HasCost.HasValue && !ServiceModel!.HasCost.Value)
                {
                    UserInput.HasCost = false;
                }
                break;
        }
    }

    protected override async Task<IActionResult> OnPostWithModelAsync(CancellationToken cancellationToken)
    {
        if (!UserInput.HasCost.HasValue)
        {
            return RedirectToSelf(UserInput, ErrorId.Service_Cost__MissingSelection);
        }

        if (!string.IsNullOrWhiteSpace(UserInput.Description) && UserInput.Description.Replace("\r", "").Length > MaxLength)
        {
            return RedirectToSelf(UserInput, ErrorId.Service_Cost_Description__TooLong);
        }

        switch (Flow)
        {
            case JourneyFlow.Edit:
                await UpdateServiceCost(UserInput.HasCost.Value, UserInput.Description!, cancellationToken);
                break;
            default:
                if (UserInput.HasCost == true)
                {
                    ServiceModel!.HasCost = true;
                    ServiceModel!.CostDescription = UserInput.Description;
                }
                else
                {
                    ServiceModel!.HasCost = false;
                    ServiceModel!.CostDescription = null;
                }
                break;
        }

        return NextPage();
    }

    private async Task UpdateServiceCost(bool hasCost, string costDescription, CancellationToken cancellationToken)
    {
        var service = await _serviceDirectoryClient.GetServiceById(ServiceId!.Value, cancellationToken);
        if (hasCost)
        {
            service.CostOptions = new List<CostOptionDto>
            {
                new()
                {
                    AmountDescription = costDescription
                }
            };
        }
        else
        {
            service.CostOptions = new List<CostOptionDto>();
        }

        await _serviceDirectoryClient.UpdateService(service, cancellationToken);
    }
}