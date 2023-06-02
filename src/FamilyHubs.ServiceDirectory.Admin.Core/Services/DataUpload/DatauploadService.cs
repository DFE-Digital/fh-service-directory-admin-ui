using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Exceptions;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Services.DataUpload.Extensions;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using Microsoft.Extensions.Logging;

namespace FamilyHubs.ServiceDirectory.Admin.Core.Services.DataUpload;

public interface IDataUploadService
{
    Task<List<string>> UploadToApi(BufferedSingleFileUploadDb fileUpload);
}

public class DataUploadService : IDataUploadService
{
    private const long IdNotSet = 0;

    private readonly IServiceDirectoryClient _serviceDirectoryClient;
    private readonly IPostcodeLocationClientService _postcodeLocationClientService;
    private readonly ILogger<DataUploadService> _logger;
    private readonly CachedApiResponses _cachedApiResponses = new CachedApiResponses();
    private readonly List<string> _errors = new List<string>();
    private readonly IExcelReader _excelReader;

    public DataUploadService(
        ILogger<DataUploadService> logger,
        IServiceDirectoryClient serviceDirectoryClient,
        IPostcodeLocationClientService postcodeLocationClientService,
        IExcelReader excelReader)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
        _postcodeLocationClientService = postcodeLocationClientService;
        _excelReader = excelReader;
        _logger = logger;
    }

    public async Task<List<string>> UploadToApi(BufferedSingleFileUploadDb fileUpload)
    {
        _logger.LogInformation($"UploadToApi Started for file - {fileUpload.FormFile.FileName}");

        var taxonomies = await _serviceDirectoryClient.GetTaxonomyList(1, 999999999);
        _cachedApiResponses.Taxonomies.AddRange(taxonomies.Items);

        var uploadData = await ParseExcelSpreadsheet(fileUpload);

        if(uploadData == null)
        {
            return _errors;
        }

        var services = await ProcessRows(uploadData);
        foreach (var service in services)
        {
            await UploadService(service);
        }

        _logger.LogInformation($"UploadToApi completed with {_errors.Count} errors for file - {fileUpload.FormFile.FileName}");
        return _errors;
    }

    private async Task<List<ServiceForUpload>> ProcessRows(IEnumerable<DataUploadRowDto> uploadData)
    {
        var servicesForUpload = new List<ServiceForUpload>();

        //  Iterate Organisations
        foreach (var localAuthorityGroupedData in uploadData.GroupBy(p => p.LocalAuthority))
        {
            var localAuthority = await GetLocalAuthority(localAuthorityGroupedData.Key);

            if (localAuthority is null)
            {
                var rows = localAuthorityGroupedData.Select(m => m.ExcelRowId).ToList();
                rows.ForEach(r => _errors.Add($"Failed to find local authority row:{r}"));
                continue;
            }

            foreach (var serviceGroupedData in localAuthorityGroupedData.GroupBy(p => p.ServiceOwnerReferenceId))
            {
                try
                {
                    var organisation = await serviceGroupedData.ResolveOrganisation(localAuthority, _cachedApiResponses, _serviceDirectoryClient);
                    var service = await ExtractService(organisation, serviceGroupedData);
                    servicesForUpload.Add(service);
                }
                catch (DataUploadException exp)
                {
                    if (exp.InnerException != null)
                        _logger.LogError(exp.InnerException, exp.Message);
                    _errors.Add(exp.Message);
                }
            }

        }

        return servicesForUpload;
    }

    private async Task<ServiceForUpload> ExtractService(OrganisationWithServicesDto organisation, IGrouping<string, DataUploadRowDto> serviceGroupedData)
    {
        var dataUploadRowDtos = serviceGroupedData.ToList();
        var serviceForUpload = new ServiceForUpload
        {
            RelatedRows = dataUploadRowDtos.Select(m => m.ExcelRowId).ToList(),
            ServiceUniqueIdentifier = serviceGroupedData.Key
        };

        var serviceOwnerReferenceIdWithPrefix = $"{organisation.AdminAreaCode.Remove(0, 1)}{serviceGroupedData.Key}";

        var existingService = organisation.Services.FirstOrDefault(x => x.ServiceOwnerReferenceId == serviceOwnerReferenceIdWithPrefix);

        if (existingService == null)
            serviceForUpload.IsNewService = true;

        var service = new ServiceDto
        {
            Id = existingService?.Id ?? IdNotSet,
            OrganisationId = organisation.Id,
            ServiceOwnerReferenceId = serviceOwnerReferenceIdWithPrefix,
            ServiceType = GetServiceType(organisation.OrganisationType),
            Name = dataUploadRowDtos.GetServiceValue(x => x.NameOfService),
            Description = dataUploadRowDtos.GetServiceValue(x => x.ServiceDescription),
            Status = ServiceStatusType.Active,
            DeliverableType = DeliverableType.NotSet,
            AttendingType = AttendingType.NotSet
        };


        foreach (var serviceRow in dataUploadRowDtos)
        {
            serviceRow.UpdateServiceDeliveries(service);
            await serviceRow.UpdateLocations(existingService, service, _postcodeLocationClientService);
            serviceRow.UpdateServiceContacts(existingService, service);
            serviceRow.UpdateLanguages(existingService, service);
            serviceRow.UpdateEligibilities(existingService, service);
            serviceRow.UpdateCosts(existingService, service);
            serviceRow.UpdateRegularSchedules(existingService, service);
            serviceRow.UpdateTaxonomies(service, _cachedApiResponses);
        }

        service.RationaliseContacts();

        serviceForUpload.Service = service;
        return serviceForUpload;
    }

    private async Task UploadService(ServiceForUpload service)
    {
        try
        {
            if (service.IsNewService)
            {
                await _serviceDirectoryClient.CreateService(service.Service!);
            }
            else
            {
                await _serviceDirectoryClient.UpdateService(service.Service!);
            }
        }
        catch (ApiException ex)
        {
            foreach (var error in ex.ApiErrorResponse.Errors)
            {
                _errors.Add($"Service {service.ServiceUniqueIdentifier} failed to be created - {error.PropertyName}:{error.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            var msg = $"Service {service.ServiceUniqueIdentifier} failed to be created";
            _logger.LogError(ex, msg);
            _errors.Add(msg);
        }
    }

    private async Task<OrganisationWithServicesDto?> GetLocalAuthority(string organisationName)
    {
        _logger.LogInformation($"Getting OrganisationWithServicesDto for Organisation {organisationName}");
        if (!_cachedApiResponses.Organisations.Any() || _cachedApiResponses.Organisations.Count(x => x.Name == organisationName) == 0)
        {
            _cachedApiResponses.Organisations = await _serviceDirectoryClient.GetListOrganisations();
        }

        var organisation = _cachedApiResponses.Organisations.FirstOrDefault(x => string.Equals(x.Name, organisationName, StringComparison.InvariantCultureIgnoreCase));
        if (organisation == null)
        {
            _logger.LogInformation($"Organisation '{organisationName}' does not exist");
            return null;
        }

        var organisationWithServices = _cachedApiResponses.OrganisationsWithServices.FirstOrDefault(o => o.Id == organisation.Id);

        if (organisationWithServices is null || organisationWithServices.Services is { Count: >= 0 })
        {
            organisationWithServices = await _serviceDirectoryClient.GetOrganisationById(organisation.Id);

            _cachedApiResponses.OrganisationsWithServices.Add(organisationWithServices!);
        }

        organisationWithServices!.AdminAreaCode = organisation.AdminAreaCode;

        _logger.LogInformation($"Returning OrganisationWithServicesDto '{organisationName}'");
        return organisationWithServices;
    }

    private static ServiceType GetServiceType(OrganisationType organisationType)
    {
        return organisationType == OrganisationType.LA 
            ? ServiceType.FamilyExperience 
            : ServiceType.InformationSharing;
    }

    private async Task<List<DataUploadRowDto>?> ParseExcelSpreadsheet(BufferedSingleFileUploadDb fileUpload)
    {
        try
        {
            return await _excelReader.GetRequestsDataFromExcel(fileUpload);
        }
        catch (DataUploadException ex)
        {
            _logger.LogWarning(ex.Message);
            _errors.Add(ex.Message); // We control these errors so safe to return to UI
            return null; 
        }
        catch (Exception ex)
        {
            _logger.LogError($"GetRequestsDataFromExcel Failed : {ex.Message}");
            _errors.Add("Failed to read data from excel spreadsheet" );
            return null;
        }
    }
}