using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models.ServiceJourney;
using FamilyHubs.ServiceDirectory.Admin.Web.Journeys;
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
        ArgumentNullException.ThrowIfNull(serviceId);

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
            Description = service.Summary,
            MoreDetails = service.Description
        };

        AddWhoFor(service, serviceModel);
        AddServiceCost(service, serviceModel);
        AddSupportOffered(service, serviceModel);
        AddTimes(service, serviceModel);
        AddLanguages(service, serviceModel);
        AddHowUse(service, serviceModel);
        AddContacts(service, serviceModel);
        AddLocations(service, serviceModel);
        //AddServiceAtLocations(service, serviceModel);

        return serviceModel;
    }

    //private void AddServiceAtLocations(ServiceDto service, ServiceModel serviceModel)
    //{
    //    serviceModel.ServiceAtLocations = service.ServiceAtLocations
    //        .Select(dto => new ServiceAtLocationModel(dto))
    //        .ToList();
    //}

    private void AddLocations(ServiceDto service, ServiceModel serviceModel)
    {
        //todo: add sat extra in here or above. zip?

        serviceModel.Locations = service.Locations
            .Select(dto => new ServiceLocationModel(dto))
            .ToList();
    }

    private void AddContacts(ServiceDto service, ServiceModel serviceModel)
    {
        var contact = service.Contacts.FirstOrDefault();
        serviceModel.HasEmail = !string.IsNullOrWhiteSpace(contact?.Email);
        serviceModel.Email = contact?.Email;
        serviceModel.HasTelephone = !string.IsNullOrWhiteSpace(contact?.Telephone);
        serviceModel.TelephoneNumber = contact?.Telephone;
        serviceModel.HasWebsite = !string.IsNullOrWhiteSpace(contact?.Url);
        serviceModel.Website = contact?.Url;
        serviceModel.HasTextMessage = !string.IsNullOrWhiteSpace(contact?.TextPhone);
        serviceModel.TextTelephoneNumber = contact?.TextPhone;
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
        var serviceSchedule = service.Schedules
            .FirstOrDefault();
        //s =>
        //        s.AttendingType == AttendingType.Online.ToString()
        //        || s.AttendingType == AttendingType.Telephone.ToString());

        serviceModel.Times = serviceSchedule?.ByDay?.Split(",")
                             ?? Enumerable.Empty<string>();

        serviceModel.TimeDescription = serviceSchedule?.Description;

        //todo: add service at location schedules here too
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