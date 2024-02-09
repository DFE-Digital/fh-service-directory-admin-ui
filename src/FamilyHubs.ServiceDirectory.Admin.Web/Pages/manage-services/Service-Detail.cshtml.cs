using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Authorization;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using Microsoft.AspNetCore.Mvc;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Factories;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

//todo: check if updated for save

[Authorize(Roles = RoleGroups.AdminRole)]
public class Service_DetailModel : ServicePageModel
{
    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public Service_DetailModel(
        IRequestDistributedCache connectionRequestCache,
        IServiceDirectoryClient serviceDirectoryClient)
        : base(ServiceJourneyPage.Service_Detail, connectionRequestCache)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    protected override async Task<IActionResult> OnPostWithModelAsync(CancellationToken cancellationToken)
    {
        if (Flow == JourneyFlow.Edit)
        {
            await UpdateService(cancellationToken);
            return RedirectToPage("/manage-services/Service-Edited-Confirmation");
        }

        await AddService(cancellationToken);
        return RedirectToPage("/manage-services/Service-Saved-Confirmation");
    }

    private Task AddService(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
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

        // times they are a-changin' so no point putting using the existing time update code in here
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
        var taxonomies = await _serviceDirectoryClient.GetTaxonomyList(1, 999999);

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

    // times they are a-changin' so no point putting using the existing time update code
    // but here it is, in case it's useful

    //private async Task UpdateWhen(TimesModels times, CancellationToken cancellationToken)
    //{
    //    var service = await _serviceDirectoryClient.GetServiceById(ServiceId!.Value, cancellationToken);

    //    var descriptionSchedule = service.Schedules.FirstOrDefault(x => x.Description != null);

    //    service.Schedules = new List<ScheduleDto>();

    //    AddToSchedule(service, DayType.Weekdays, times.WeekdaysStarts, times.WeekdaysFinishes);
    //    AddToSchedule(service, DayType.Weekends, times.WeekendsStarts, times.WeekendsFinishes);

    //    if (descriptionSchedule != null)
    //    {
    //        service.Schedules.Add(descriptionSchedule);
    //    }

    //    await _serviceDirectoryClient.UpdateService(service, cancellationToken);
    //}

    //private static void AddToSchedule(ServiceDto service, DayType days, TimeModel starts, TimeModel finishes)
    //{
    //    var startTime = starts.ToDateTime();
    //    var finishesTime = finishes.ToDateTime();
    //    if (startTime == null || finishesTime == null)
    //    {
    //        return;
    //    }

    //    //todo: throw if one but not the other?

    //    service.Schedules.Add(new ScheduleDto
    //    {
    //        Freq = FrequencyType.Weekly,
    //        ByDay = days == DayType.Weekdays ? ByDayWeekdays : ByDayWeekends,
    //        OpensAt = startTime,
    //        ClosesAt = finishesTime
    //    });
    //}
}