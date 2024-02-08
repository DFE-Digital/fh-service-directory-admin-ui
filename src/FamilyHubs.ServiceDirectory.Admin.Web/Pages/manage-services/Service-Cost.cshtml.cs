using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class ServiceCostUserInput
{
    public bool? HasCost { get; set; }
    public string? Description { get; set; }
}

public class Service_CostModel : ServicePageModel<ServiceCostUserInput>
{
    public string TextBoxLabel { get; set; } = "Does the service cost money to use?";
    public int? MaxLength => 150;

    [BindProperty]
    public ServiceCostUserInput UserInput { get; set; } = new();

    public Service_CostModel(IRequestDistributedCache connectionRequestCache)
    : base(ServiceJourneyPage.Service_Cost, connectionRequestCache)
    {
    }

    protected override void OnGetWithModel()
    {
        if (Errors.HasErrors)
        {
            UserInput = ServiceModel!.UserInput!;
            return;
        }

        if (ServiceModel!.HasCost == true)
        {
            UserInput.HasCost = true;
            UserInput.Description = ServiceModel!.CostDescription!;
        }
        else if (ServiceModel!.HasCost.HasValue && !ServiceModel!.HasCost.Value)
        {
            UserInput.HasCost = false;
        }
    }

    protected override IActionResult OnPostWithModel()
    {
        if (!UserInput.HasCost.HasValue)
        {
            return RedirectToSelf(UserInput, ErrorId.Service_Cost__MissingSelection);
        }

        //todo: use the component code for this check?
        if (!string.IsNullOrWhiteSpace(UserInput.Description) && UserInput.Description.Replace("\r", "").Length > MaxLength)
        {
            return RedirectToSelf(UserInput, ErrorId.Service_Cost__DescriptionTooLong);
        }

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

        return NextPage();
    }
}