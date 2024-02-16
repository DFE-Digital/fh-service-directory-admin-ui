using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Journeys;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class start_edit_serviceModel : PageModel
{
    private readonly IRequestDistributedCache _cache;
    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public start_edit_serviceModel(
        IRequestDistributedCache cache,
        IServiceDirectoryClient serviceDirectoryClient)
    {
        _cache = cache;
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    public async Task<IActionResult> OnGetAsync(long? serviceId)
    {
        if (serviceId == null)
        {
            throw new ArgumentNullException(nameof(serviceId));
        }

        var service = await _serviceDirectoryClient.GetServiceById(serviceId.Value);

        var familyHubsUser = HttpContext.GetFamilyHubsUser();

        // the user's just starting the journey
        await _cache.SetAsync(familyHubsUser.Email, CreateServiceModel(serviceId.Value, service));

        return Redirect(ServiceJourneyPageExtensions.GetEditStartPagePath());
    }

    private ServiceModel CreateServiceModel(long serviceId, ServiceDto service)
    {
        var serviceModel = new ServiceModel
        {
            Id = serviceId,
            Name = service.Name,
            Description = service.Description
        };

        AddWhoFor(service, serviceModel);
        AddServiceCost(service, serviceModel);
        AddSupportOffered(service, serviceModel);
        AddTimeDetails(service, serviceModel);
        AddTimes(service, serviceModel);
        AddLanguages(service, serviceModel);
        AddHowUse(service, serviceModel);

        return serviceModel;
    }

    private static void AddLanguages(ServiceDto service, ServiceModel serviceModel)
    {
        serviceModel.LanguageCodes = service.Languages
            .Select(l => l.Code)
            .ToList();

        //todo: move to sd shared?
        service.InterpretationServices?.Split(',').ToList().ForEach(s =>
        {
            switch (s)
            {
                //todo: magic strings in service directory shared
                case "translation":
                    serviceModel.TranslationServices = true;
                    break;
                case "bsl":
                    serviceModel.BritishSignLanguage = true;
                    break;
            }
        });
    }

    private static void AddHowUse(ServiceDto service, ServiceModel serviceModel)
    {
        serviceModel.HowUse = service.ServiceDeliveries
            .Select(sd => sd.Name)
            .ToArray();
    }

    private static void AddTimes(ServiceDto service, ServiceModel serviceModel)
    {
        serviceModel.Times = service.Schedules
            .FirstOrDefault(s => s is { Freq: FrequencyType.WEEKLY })
            ?.ByDay
            ?.Split(",") ?? Enumerable.Empty<string>();
    }

    private static void AddTimeDetails(ServiceDto service, ServiceModel serviceModel)
    {
        serviceModel.TimeDescription = service.Schedules
            .FirstOrDefault(x => x.Description != null)?
            .Description;
        serviceModel.HasTimeDetails = serviceModel.TimeDescription != null;
    }

    private static void AddSupportOffered(ServiceDto service, ServiceModel serviceModel)
    {
        serviceModel.SelectedCategories = service.Taxonomies
            .Select(x => x.ParentId)
            .Distinct()
            .ToList();
        serviceModel.SelectedSubCategories = service.Taxonomies
            .Select(x => x.Id)
            .ToList();
    }

    private static void AddServiceCost(ServiceDto service, ServiceModel serviceModel)
    {
        serviceModel.HasCost = service.CostOptions.Count > 0;
        if (serviceModel.HasCost == true)
        {
            serviceModel.CostDescription = service.CostOptions.First().AmountDescription!;
        }
    }

    private static void AddWhoFor(ServiceDto service, ServiceModel serviceModel)
    {
        var eligibility = service.Eligibilities.FirstOrDefault();
        serviceModel.ForChildren = eligibility != null;
        if (serviceModel.ForChildren == true)
        {
            serviceModel.MinimumAge = eligibility!.MinimumAge;
            serviceModel.MaximumAge = eligibility.MaximumAge;
        }
    }
}