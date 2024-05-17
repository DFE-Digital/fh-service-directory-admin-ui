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

    public string? OrganisationName { get; private set; }
    public string? LaOrganisationName { get; private set; }

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

    protected override string GenerateBackUrl()
    {
        if (Flow == JourneyFlow.Edit && ChangeFlow == null)
        {
            return GenerateBackUrlToJourneyInitiatorPage();
        }

        ServiceJourneyPage? back = BackParam;
        if (back == null)
        {
            throw new InvalidOperationException("Back page not supplied as param");
        }
        return GetServicePageUrl(back.Value);
    }

    protected override async Task OnGetWithModelAsync(CancellationToken cancellationToken)
    {
        await PopulateTaxonomyIdToName(cancellationToken);
        await PopulateFromOrganisations(cancellationToken);

        await ClearErrors();

        SetDoNotCacheHeaders();

        //todo: really need to do something similar when the user adds a location, then goes back to the locations for service page

        //todo: need to add org id and la org id to mini journey cache for la mini journey

        if (ServiceModel!.FinishingJourney == true)
        {
            // accept the changes made during the mini journey
            ServiceModel.AcceptMiniJourneyChanges();
        }
        else
        {
            // the user has come back to the page by using the back button
            // or they've just landed on the page at the start of the edit journey (in which case this is a no-op)
            // or the user has clicked reload page
            ServiceModel.RestoreMiniJourneyCopyIfExists();
        }

        // only really needs to be done when starting how use/locations mini journey
        ServiceModel!.SaveMiniJourneyCopy();
        await Cache.SetAsync(FamilyHubsUser.Email, ServiceModel);
    }

    private void SetDoNotCacheHeaders()
    {
        // we always need the browser to come back to the server
        // when the user comes back to this page, after hitting the browser (or page) back button.
        // so we tell the browser not to cache the page
        Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
        Response.Headers.Add("Pragma", "no-cache");
        Response.Headers.Add("Expires", "0");
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

    private async Task PopulateFromOrganisations(CancellationToken cancellationToken)
    {
        List<Task<OrganisationDetailsDto>> tasks = new()
        {
            _serviceDirectoryClient.GetOrganisationById(ServiceModel!.OrganisationId!.Value, cancellationToken),
        };

        if (FamilyHubsUser.Role == RoleTypes.DfeAdmin && ServiceModel.ServiceType == ServiceTypeArg.Vcs)
        {
            tasks.Add(_serviceDirectoryClient.GetOrganisationById(ServiceModel.LaOrganisationId!.Value, cancellationToken));
        }

        await Task.WhenAll(tasks);

        var organisation = tasks[0].Result;

        if (FamilyHubsUser.Role != RoleTypes.DfeAdmin)
        {
            ServiceModel.ServiceType = GetServiceTypeArgFromOrganisation(organisation);
        }
        else
        {
            if (ServiceModel.ServiceType == ServiceTypeArg.La)
            {
                LaOrganisationName = organisation.Name;
            }
            else
            {
                LaOrganisationName = tasks[1].Result.Name;
                OrganisationName = organisation.Name;
            }
        }
    }

    //private async Task<string> GetOrganisationName(long organisationId, CancellationToken cancellationToken)
    //{
    //    var organisation = await _serviceDirectoryClient.GetOrganisationById(organisationId, cancellationToken);

    //    return organisation.Name;
    //}

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
        //todo: store org type and name in cache

        if (Flow == JourneyFlow.Edit)
        {
            //var organisation = await _serviceDirectoryClient.GetOrganisationById(ServiceModel!.OrganisationId!.Value, cancellationToken);

            await UpdateService(cancellationToken);
            return RedirectToPage("/manage-services/Service-Edit-Confirmation");
        }

        //long organisationId = GetUsersOrganisationId();
        //var userOrganisation = await _serviceDirectoryClient.GetOrganisationById(organisationId, cancellationToken);

        await AddService(cancellationToken);
        return RedirectToPage("/manage-services/Service-Add-Confirmation");
    }

    private async Task<long> AddService(CancellationToken cancellationToken)
    {
        var service = CreateServiceChangeDto();

        return await _serviceDirectoryClient.CreateService(service, cancellationToken);
    }

    //private long GetUsersOrganisationId()
    //{
    //    switch (FamilyHubsUser.Role)
    //    {
    //        case RoleTypes.LaManager:
    //        case RoleTypes.LaDualRole:
    //        case RoleTypes.VcsManager:
    //        case RoleTypes.VcsDualRole:
    //            return long.Parse(FamilyHubsUser.OrganisationId);
    //        case RoleTypes.DfeAdmin:
    //            return ServiceModel!.OrganisationId.Value;
    //            break;
    //        default:
    //            throw new InvalidOperationException($"User role not supported: {FamilyHubsUser.Role}");
    //    }
    //}

    private async Task UpdateService(CancellationToken cancellationToken)
    {
        var serviceChange = CreateServiceChangeDto(ServiceModel!.Id!.Value);

        await _serviceDirectoryClient.UpdateService(serviceChange, cancellationToken);
    }

    //naming/combine?
    private ServiceChangeDto CreateServiceChangeDto(long? serviceId = null)
    {
        var serviceChangeDto = new ServiceChangeDto
        {
            Name = ServiceModel!.Name!,
            Summary = ServiceModel.Description,
            Description = ServiceModel.MoreDetails,
            //todo: can come from ServiceTypeArg, as long as set it for edit
            //ServiceType = GetServiceType(organisation),
            ServiceType = GetServiceType(),
            Status = ServiceStatusType.Active,
            OrganisationId = ServiceModel.OrganisationId!.Value,
            InterpretationServices = GetInterpretationServices(),
            // collections
            CostOptions = GetServiceCost(),
            Languages = GetLanguages(),
            Eligibilities = GetEligibilities(),
            Schedules = GetSchedules(),
            TaxonomyIds = ServiceModel.SelectedSubCategories,
            Contacts = GetContacts(),
            ServiceDeliveries = GetServiceDeliveries(),
            ServiceAtLocations = ServiceModel.AllLocations.Select(Map).ToArray()
        };

        if (serviceId != null)
        {
            serviceChangeDto.Id = serviceId.Value;
        }

        return serviceChangeDto;
    }

    private static string? GetByDay(IEnumerable<string>? times)
    {
        return times == null ? null : string.Join(',', times);
    }

    private ServiceAtLocationDto Map(ServiceLocationModel serviceAtLocation)
    {
        return new ServiceAtLocationDto
        {
            LocationId = serviceAtLocation.Id,
            Schedules = new List<ScheduleDto>
            {
                CreateSchedule(GetByDay(serviceAtLocation.Times!), serviceAtLocation.TimeDescription, AttendingType.InPerson)
            }
        };
    }

    private ServiceType GetServiceType()
    {
        // this logic only holds whilst LA's can only create FamilyExperience services
        return ServiceModel!.ServiceType switch
        {
            ServiceTypeArg.La => ServiceType.FamilyExperience,
            ServiceTypeArg.Vcs => ServiceType.InformationSharing,
            _ => throw new InvalidOperationException($"Unexpected ServiceType {ServiceModel.ServiceType}")
        };
    }

    //private static ServiceType GetServiceTypeFromOrganisation(OrganisationDto organisation)
    //{
    //    // this logic only holds whilst LA's can only create FamilyExperience services
    //    return organisation.OrganisationType switch
    //    {
    //        OrganisationType.LA => ServiceType.FamilyExperience,
    //        OrganisationType.VCFS => ServiceType.InformationSharing,
    //        _ => throw new InvalidOperationException($"Organisation type not supported: {organisation.OrganisationType}")
    //    };
    //}

    private static ServiceTypeArg GetServiceTypeArgFromOrganisation(OrganisationDto organisation)
    {
        // this logic only holds whilst LA's can only create FamilyExperience services
        return organisation.OrganisationType switch
        {
            OrganisationType.LA => ServiceTypeArg.La,
            OrganisationType.VCFS => ServiceTypeArg.Vcs,
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
                // as the international standard has optional phone entities with mandatory numbers (so effectively phones are optional)
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