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
        //todo: move into method?
        if (TaxonomyIdToName == null)
        {
            // without locking, TaxonomyIdToName might get initialized more than once, but that's not the end of the world

            var allTaxonomies = await _taxonomyService.GetCategories(cancellationToken);

            TaxonomyIdToName = allTaxonomies
                .SelectMany(x => x.Value)
                .ToDictionary(t => t.Id, t => t.Name);
        }

        //MoveCurrentLocationToLocations();
    }

    //private void MoveCurrentLocationToLocations()
    //{
    //    if (ServiceModel!.CurrentLocation != null)
    //    {
    //        ServiceModel.Locations.Add(ServiceModel.CurrentLocation);
    //        ServiceModel.CurrentLocation = null;
    //    }
    //}

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

    private async Task<long> AddService(CancellationToken cancellationToken)
    {
        var organisation = await GetServiceOrganisation(cancellationToken);

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

    private Task UpdateService(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();

        // will have to revisit the update
        // will probably still have to get the existing service
        // (for existing objects in the graph that will need updating)
        // some 

        //long serviceId = ServiceModel!.Id!.Value;
        //var service = await _serviceDirectoryClient.GetServiceById(serviceId, cancellationToken);
        //if (service is null)
        //{
        //    //todo: better exception?
        //    throw new InvalidOperationException($"Service not found: {serviceId}");
        //}

        //await UpdateServiceFromCache(service, cancellationToken);

        //await _serviceDirectoryClient.UpdateService(service, cancellationToken);
    }

    private ServiceChangeDto CreateServiceChangeDtoFromCache(OrganisationDto organisation)
    {
        return new ServiceChangeDto
        {
            Name = ServiceModel!.Name!,
            Description = ServiceModel.Description,
            ServiceType = GetServiceType(organisation),
            //todo: remove from schema
            ServiceOwnerReferenceId = "",
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

    private static string GetByDay(IEnumerable<string> times)
    {
        return string.Join(',', times);
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

    private void UpdateLocations(ServiceDto service)
    {
        //todo: api will only use Id, but there are a bunch of required fields
        // how to best handle it?
        // don't really want to put in a lot of dummy values, although it would work
        // load locations for db: would work, but slow and not necessary, especially if update just works with ids
        // separate dto for create and update?
        // use original entities (or dtos), but use inheritance where base just contains the id?
        //service.Locations = ServiceModel!.AllLocations
        //    .Select(l => new LocationDto { Id = l.Id })
        //    .ToList();

        throw new NotImplementedException();
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

        string byDay = GetByDay(ServiceModel!.Times!);

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
        string byDay,
        string? timeDescription,
        AttendingType? attendingType)
    {
        var schedule = new ScheduleDto
        {
            AttendingType = attendingType.ToString(),
            Description = timeDescription
        };

        if (byDay != "")
        {
            schedule.Freq = FrequencyType.WEEKLY;
            schedule.ByDay = byDay;
        }

        return schedule;
    }
}