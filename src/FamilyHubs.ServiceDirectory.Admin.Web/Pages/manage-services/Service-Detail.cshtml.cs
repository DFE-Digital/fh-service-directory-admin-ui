using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Authorization;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using Microsoft.AspNetCore.Mvc;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Factories;
using FamilyHubs.ServiceDirectory.Shared.Enums;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

[Authorize(Roles = RoleGroups.AdminRole)]
public class Service_DetailModel : ServicePageModel
{
    public List<LocationDto> Locations { get; private set; }

    public static IReadOnlyDictionary<long, string>? TaxonomyIdToName { get; set; }

    private readonly IServiceDirectoryClient _serviceDirectoryClient;
    private readonly ITaxonomyService _taxonomyService;

    public Service_DetailModel(
        IRequestDistributedCache connectionRequestCache,
        IServiceDirectoryClient serviceDirectoryClient,
        ITaxonomyService taxonomyService)
        : base(ServiceJourneyPage.Service_Detail, connectionRequestCache)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
        _taxonomyService = taxonomyService;
    }

    protected override async Task OnGetWithModelAsync(CancellationToken cancellationToken)
    {
        var allTaxonomies = await _taxonomyService.GetCategories(cancellationToken);

        // without locking, it might get initialized more than once, but that's fine
        TaxonomyIdToName ??= allTaxonomies
            .SelectMany(x => x.Value)
            .ToDictionary(t => t.Id, t => t.Name);

        //if (!ServiceModel!.HowUse.Contains(AttendingType.InPerson))
        //{
        //    ServiceModel.CurrentLocation = null;
        //}

        //todo: this will end up with a foreach
        Locations = new List<LocationDto>();
        if (ServiceModel!.CurrentLocation != null)
        {
            Locations.Add(await _serviceDirectoryClient.GetLocationById(ServiceModel.CurrentLocation.Value, cancellationToken));
        }
    }

    protected override async Task<IActionResult> OnPostWithModelAsync(CancellationToken cancellationToken)
    {
        if (Flow == JourneyFlow.Edit)
        {
            await UpdateService(cancellationToken);
            return RedirectToPage("/manage-services/Service-Edit-Confirmation");
        }

        await AddService(cancellationToken);
        return RedirectToPage("/manage-services/Service-Add-Confirmation");
    }

    private Task<long> AddService(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();

        //var service = new ServiceDto
        //{
        //    // required, but will be replaced
        //    Name = "",
        //    ServiceType = ServiceType.FamilyExperience,
        //    ServiceOwnerReferenceId = "",
        //    CostOptions = new List<CostOptionDto>(),
        //    Languages = new List<LanguageDto>(),
        //    Eligibilities = new List<EligibilityDto>(),
        //    Schedules = new List<ScheduleDto>(),
        //    Taxonomies = new List<TaxonomyDto>()
        //};

        //await UpdateServiceFromCache(service, cancellationToken);

        //return await _serviceDirectoryClient.CreateService(service, cancellationToken);
    }

    private async Task UpdateService(CancellationToken cancellationToken)
    {
        long serviceId = ServiceModel!.Id!.Value;
        var service = await _serviceDirectoryClient.GetServiceById(serviceId, cancellationToken);
        if (service is null)
        {
            //todo: better exception?
            throw new InvalidOperationException($"Service not found: {serviceId}");
        }

        await UpdateServiceFromCache(service, cancellationToken);

        await _serviceDirectoryClient.UpdateService(service, cancellationToken);
    }

    private async Task UpdateServiceFromCache(ServiceDto service, CancellationToken cancellationToken)
    {
        service.Name = ServiceModel!.Name!;
        service.Description = ServiceModel.Description;

        await UpdateTaxonomies(service, cancellationToken);
        UpdateServiceCost(service);
        UpdateLanguages(service);
        UpdateEligibility(service);
        UpdateWhen(service);
        UpdateHowUse(service);
        UpdateLocations(service);
        UpdateContacts(service);
    }

    private void UpdateContacts(ServiceDto service)
    {
        service.Contacts = new List<ContactDto>()
        {
            new()
            {
                Email = ServiceModel!.HasEmail ? ServiceModel.Email : "",
                Telephone = ServiceModel!.HasTelephone ? ServiceModel!.TelephoneNumber! : "",
                Url = ServiceModel!.HasWebsite ? ServiceModel.Website : "",
                TextPhone = ServiceModel!.HasTextMessage ? ServiceModel.TextTelephoneNumber : ""
            }
        };
    }

    private void UpdateLocations(ServiceDto service)
    {
        //todo: will need to update API - we just need to add the location ids
        // (we could fetch the locations and add them, but that's not necessary)
        //service.Locations = ServiceModel!.LocationIds
        //    .Select(l => new LocationDto { Id = l })
        //    .ToList();
    }

    private void UpdateHowUse(ServiceDto service)
    {
        service.ServiceDeliveries = ServiceModel!.HowUse
            .Select(hu => new ServiceDeliveryDto { Name = hu })
            .ToArray();
    }

    private void UpdateServiceCost(ServiceDto service)
    {
        if (ServiceModel!.HasCost == true)
        {
            service.CostOptions = new List<CostOptionDto>
            {
                new()
                {
                    AmountDescription = ServiceModel.CostDescription
                }
            };
        }
        else
        {
            service.CostOptions = new List<CostOptionDto>();
        }
    }

    private async Task UpdateTaxonomies(ServiceDto service, CancellationToken cancellationToken)
    {
        //todo: update to accept cancellation token
        var taxonomies = await _serviceDirectoryClient.GetTaxonomyList(1, 999999, cancellationToken: cancellationToken);

        var selectedTaxonomies = taxonomies.Items
            .Where(x => ServiceModel!.SelectedSubCategories.Contains(x.Id))
            .ToList();

        service.Taxonomies = selectedTaxonomies;
    }

    private void UpdateLanguages(ServiceDto service)
    {
        var interpretationServices = new List<string>();
        if (ServiceModel!.TranslationServices == true)
        {
            interpretationServices.Add("translation");
        }
        if (ServiceModel.BritishSignLanguage == true)
        {
            interpretationServices.Add("bsl");
        }

        service.InterpretationServices = string.Join(',', interpretationServices);

        service.Languages = ServiceModel.LanguageCodes!.Select(LanguageDtoFactory.Create).ToList();
    }

    private void UpdateEligibility(ServiceDto service)
    {
        if (ServiceModel!.ForChildren == true)
        {
            //todo: when adding, will need to add to Eligibilities?
            var eligibility = service.Eligibilities.FirstOrDefault();
            if (eligibility == null)
            {
                service.Eligibilities.Add(new EligibilityDto
                {
                    MinimumAge = ServiceModel.MinimumAge!.Value,
                    MaximumAge = ServiceModel.MaximumAge!.Value
                });
            }
            else
            {
                eligibility.MinimumAge = ServiceModel.MinimumAge!.Value;
                eligibility.MaximumAge = ServiceModel.MaximumAge!.Value;
            }
        }
        else
        {
            service.Eligibilities.Clear();
        }
    }

    private void UpdateWhen(ServiceDto service)
    {
        service.Schedules = new List<ScheduleDto>();

        var byDay = string.Join(',', ServiceModel!.Times!);

        foreach (var attendingType in ServiceModel.HowUse)
                     //.Where(at => at is AttendingType.Online or AttendingType.Telephone))
        {
            service.Schedules.Add(CreateSchedule(byDay, attendingType));
        }
    }

    private ScheduleDto CreateSchedule(string byDay, AttendingType attendingType)
    {
        var schedule = new ScheduleDto
        {
            AttendingType = attendingType.ToString(),
            Description = ServiceModel!.TimeDescription
        };

        if (byDay != "")
        {
            schedule.Freq = FrequencyType.WEEKLY;
            schedule.ByDay = byDay;
        }

        return schedule;
    }
}