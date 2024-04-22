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
using FamilyHubs.ServiceDirectory.Admin.Core.Models.ServiceJourney;
using FamilyHubs.ServiceDirectory.Shared.CreateUpdateDto;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

// in person, add locs, select loc, back to select loc, back => should be add locs? but locations for service page

[Authorize(Roles = RoleGroups.AdminRole)]
public class Service_DetailModel : ServicePageModel
{
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
        await PopulateTaxonomyIdToName(cancellationToken);

        await ClearErrors();
    }

    private async Task PopulateTaxonomyIdToName(CancellationToken cancellationToken)
    {
        if (TaxonomyIdToName == null)
        {
            // without locking, TaxonomyIdToName might get initialized more than once, but that's not the end of the world

            var allTaxonomies = await _taxonomyService.GetCategories(cancellationToken);

            TaxonomyIdToName = allTaxonomies
                .SelectMany(x => x.Value)
                .ToDictionary(t => t.Id, t => t.Name);
        }
    }

    /// <summary>
    /// Clear down any user errors to handle the case where:
    /// user clicks change to go back to a previous page in the journey,
    /// they click continue on that page and errors are displayed due to validation,
    /// they click back to the details page, then click 'Change' to go back to the same page
    /// and the validation errors from the first redo visit are shown.
    /// </summary>
    /// <returns></returns>
    private Task ClearErrors()
    {
        ServiceModel!.ClearErrors();
        return Cache.SetAsync(FamilyHubsUser.Email, ServiceModel);
    }

    protected override async Task<IActionResult> OnPostWithModelAsync(CancellationToken cancellationToken)
    {
        var organisation = await GetServiceOrganisation(cancellationToken);

        if (Flow == JourneyFlow.Edit)
        {
            await UpdateService(organisation, cancellationToken);
            return RedirectToPage("/manage-services/Service-Edit-Confirmation");
        }

        await AddService(organisation, cancellationToken);
        return RedirectToPage("/manage-services/Service-Add-Confirmation");
    }

    private async Task<long> AddService(OrganisationDto organisation, CancellationToken cancellationToken)
    {
        var service = CreateServiceChangeDtoFromCache(organisation);

        return await _serviceDirectoryClient.CreateService(service, cancellationToken);
    }

    private async Task<OrganisationDto> GetServiceOrganisation(CancellationToken cancellationToken)
    {
        long organisationId;
        switch (FamilyHubsUser.Role)
        {
            case RoleTypes.LaManager:
            case RoleTypes.LaDualRole:
            case RoleTypes.VcsManager:
            case RoleTypes.VcsDualRole:
                organisationId = long.Parse(FamilyHubsUser.OrganisationId);
                break;
            //todo: once we have the select org page, we'll use the selected org
            //case RoleTypes.DfeAdmin:
            //    organisationId = ServiceModel!.OrganisationId.Value;
            //    break;
            default:
                throw new InvalidOperationException($"User role not supported: {FamilyHubsUser.Role}");
        }

        return await _serviceDirectoryClient.GetOrganisationById(organisationId, cancellationToken);
    }

    private async Task UpdateService(OrganisationDto organisation, CancellationToken cancellationToken)
    {
        var service = await GetService(cancellationToken);

        var serviceChange = CreateServiceChangeDto(service, organisation); //, cancellationToken);

        await _serviceDirectoryClient.UpdateService(serviceChange, cancellationToken);
    }

    private async Task<ServiceDto> GetService(CancellationToken cancellationToken)
    {
        long serviceId = ServiceModel!.Id!.Value;
        var service = await _serviceDirectoryClient.GetServiceById(serviceId, cancellationToken);
        if (service is null)
        {
            throw new InvalidOperationException($"Service not found: {serviceId}");
        }

        return service;
    }

    //naming/combine?
    private ServiceChangeDto CreateServiceChangeDto(ServiceDto service, OrganisationDto organisation)
    {
        //todo: what happens if we ignore existing entities?
        return new ServiceChangeDto
        {
            Id = service.Id,
            Name = ServiceModel!.Name!,
            Summary = ServiceModel.Description,
            Description = ServiceModel.MoreDetails,
            ServiceType = GetServiceType(organisation),
            Status = ServiceStatusType.Active,
            CostOptions = GetUpdatedServiceCost(service),
            InterpretationServices = GetInterpretationServices(),
            Languages = GetLanguages(),
            Eligibilities = GetEligibilities(),
            Schedules = GetSchedules(),
            TaxonomyIds = ServiceModel.SelectedSubCategories,
            Contacts = GetContacts(),
            ServiceDeliveries = GetServiceDeliveries(),
            ServiceAtLocations = ServiceModel.AllLocations.Select(Map).ToArray(),
            OrganisationId = organisation.Id
        };
    }

    private ServiceChangeDto CreateServiceChangeDtoFromCache(OrganisationDto organisation)
    {
        return new ServiceChangeDto
        {
            Name = ServiceModel!.Name!,
            Summary = ServiceModel.Description,
            Description = ServiceModel.MoreDetails,
            ServiceType = GetServiceType(organisation),
            Status = ServiceStatusType.Active,
            CostOptions = GetServiceCost(),
            InterpretationServices = GetInterpretationServices(),
            Languages = GetLanguages(),
            Eligibilities = GetEligibilities(),
            Schedules = GetSchedules(),
            TaxonomyIds = ServiceModel.SelectedSubCategories,
            Contacts = GetContacts(),
            ServiceDeliveries = GetServiceDeliveries(),
            ServiceAtLocations = ServiceModel.AllLocations.Select(Map).ToArray(),
            OrganisationId = organisation.Id
        };
    }

    private static string? GetByDay(IEnumerable<string>? times)
    {
        return times == null ? null : string.Join(',', times);
    }

    private ServiceAtLocationChangeDto Map(ServiceLocationModel serviceAtLocation)
    {
        return new ServiceAtLocationChangeDto
        {
            LocationId = serviceAtLocation.Id,
            Schedules = new List<ScheduleDto>
            {
                CreateSchedule(GetByDay(serviceAtLocation.Times!), serviceAtLocation.TimeDescription, AttendingType.InPerson)
            }
        };
    }

    private static ServiceType GetServiceType(OrganisationDto organisation)
    {
        return organisation.OrganisationType switch 
        {
            OrganisationType.LA => ServiceType.FamilyExperience,
            OrganisationType.VCFS => ServiceType.InformationSharing,
            _ => throw new InvalidOperationException($"Organisation type not supported: {organisation.OrganisationType}")
        };
    }

    private List<ContactDto> GetContacts()
    {
        return new List<ContactDto>()
        {
            new()
            {
                Email = ServiceModel!.Email,
                //todo: telephone should really be nullable, but there's no point doing it now,
                // as the internation standard, has optional phone entities with mandatory numbers (so effectively phones are optional)
                Telephone = ServiceModel!.HasTelephone ? ServiceModel!.TelephoneNumber! : "",
                Url = ServiceModel.Website,
                TextPhone = ServiceModel.TextTelephoneNumber
            }
        };
    }

    //todo: consistence with type of returns : return most specific instance. can a collection be set by an enumerable?
    private ICollection<ServiceDeliveryDto> GetServiceDeliveries()
    {
        return ServiceModel!.HowUse
            .Select(hu => new ServiceDeliveryDto { Name = hu })
            .ToArray();
    }

    private List<CostOptionDto> GetServiceCost()
    {
        if (ServiceModel!.HasCost == true)
        {
            return new List<CostOptionDto>
            {
                new()
                {
                    AmountDescription = ServiceModel.CostDescription
                }
            };
        }

        return new List<CostOptionDto>();
    }

    private ICollection<CostOptionDto> GetUpdatedServiceCost(ServiceDto existingService)
    {
        if (ServiceModel!.HasCost != true)
        {
            return new List<CostOptionDto>();
        }

        if (existingService.CostOptions.Any())
        {
            // assumes only a single option, which is valid for our services, but might not be for imported services?
            existingService.CostOptions.First().AmountDescription = ServiceModel.CostDescription;
        }
        else
        {
            existingService.CostOptions.Add(new CostOptionDto
            {
                AmountDescription = ServiceModel.CostDescription
            });
        }

        return existingService.CostOptions;

    }

    private string GetInterpretationServices()
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

        return string.Join(',', interpretationServices);
    }


    private ICollection<LanguageDto> GetLanguages()
    {
        return ServiceModel!.LanguageCodes!.Select(LanguageDtoFactory.Create).ToList();
    }

    private ICollection<EligibilityDto> GetEligibilities()
    {
        var eligibilities = new List<EligibilityDto>();

        if (ServiceModel!.ForChildren == true)
        {
            eligibilities.Add(new EligibilityDto
            {
                MinimumAge = ServiceModel.MinimumAge!.Value,
                MaximumAge = ServiceModel.MaximumAge!.Value
            });
        }

        return eligibilities;
    }

    private ICollection<EligibilityDto> GetUpdatedEligibilities()
    {
        throw new NotImplementedException();
        //if (ServiceModel!.ForChildren == true)
        //{
        //    //todo: when adding, will need to add to Eligibilities?
        //    var eligibility = service.Eligibilities.FirstOrDefault();
        //    if (eligibility == null)
        //    {
        //        service.Eligibilities.Add(new EligibilityDto
        //        {
        //            MinimumAge = ServiceModel.MinimumAge!.Value,
        //            MaximumAge = ServiceModel.MaximumAge!.Value
        //        });
        //    }
        //    else
        //    {
        //        eligibility.MinimumAge = ServiceModel.MinimumAge!.Value;
        //        eligibility.MaximumAge = ServiceModel.MaximumAge!.Value;
        //    }
        //}
        //else
        //{
        //    service.Eligibilities.Clear();
        //}
    }

    // https://dfedigital.atlassian.net/browse/FHG-4829?focusedCommentId=79339
    private List<ScheduleDto> GetSchedules()
    {
        var schedules = new List<ScheduleDto>();

        string? byDay = GetByDay(ServiceModel!.Times!);

        if (ServiceModel!.HowUse.Contains(AttendingType.InPerson)
            && !ServiceModel.AllLocations.Any())
        {
            schedules.Add(CreateSchedule(byDay, ServiceModel.TimeDescription, AttendingType.InPerson));
        }

        foreach (var attendingType in ServiceModel.HowUse
                     .Where(at => at is AttendingType.Online or AttendingType.Telephone))
        {
            schedules.Add(CreateSchedule(byDay, ServiceModel.TimeDescription, attendingType));
        }

        return schedules;
    }

    private static ScheduleDto CreateSchedule(
        string? byDay,
        string? timeDescription,
        AttendingType? attendingType)
    {
        var schedule = new ScheduleDto
        {
            AttendingType = attendingType.ToString(),
            Description = timeDescription
        };

        if (!string.IsNullOrEmpty(byDay))
        {
            schedule.Freq = FrequencyType.WEEKLY;
            schedule.ByDay = byDay;
        }

        return schedule;
    }
}