using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Razor.Time;
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

    //todo: move to service directory shared
    private const string ByDayWeekdays = "MO,TU,WE,TH,FR";
    private const string ByDayWeekends = "SA,SU";

    private ServiceModel CreateServiceModel(long serviceId, ServiceDto service)
    {
        var serviceModel = new ServiceModel
        {
            Id = serviceId,
            Name = service.Name,
            Description = service.Description
        };

        //todo: extract
        var eligibility = service.Eligibilities.FirstOrDefault();
        serviceModel.ForChildren = eligibility != null;
        if (serviceModel.ForChildren == true)
        {
            serviceModel.MinimumAge = eligibility!.MinimumAge;
            serviceModel.MaximumAge = eligibility.MaximumAge;
        }

        //todo: extract
        serviceModel.HasCost = service.CostOptions.Count > 0;
        if (serviceModel.HasCost == true)
        {
            serviceModel.CostDescription = service.CostOptions.First().AmountDescription!;
        }

        //todo: extract (x2?)
        serviceModel.SelectedCategories = service.Taxonomies
            .Select(x => x.ParentId)
            .Distinct()
            .ToList();
        serviceModel.SelectedSubCategories = service.Taxonomies
            .Select(x => x.Id)
            .ToList();

        //todo: extract
        serviceModel.TimeDescription = service.Schedules
            .FirstOrDefault(x => x.Description != null)?
            .Description;
        serviceModel.HasTimeDetails = serviceModel.TimeDescription != null;

        //todo: extract
        var weekday = service.Schedules
            .FirstOrDefault(s => s is { Freq: FrequencyType.Weekly, ByDay: ByDayWeekdays });

        var weekend = service.Schedules
            .FirstOrDefault(s => s is { Freq: FrequencyType.Weekly, ByDay: ByDayWeekends });

        //todo: new TimesModels constructor
        serviceModel.Times = new TimesModels(weekday != null, weekend != null,
            new TimeModel(weekday?.OpensAt), new TimeModel(weekday?.ClosesAt),
            new TimeModel(weekend?.OpensAt), new TimeModel(weekend?.ClosesAt));

        //todo: extract
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

        return serviceModel;
    }
}