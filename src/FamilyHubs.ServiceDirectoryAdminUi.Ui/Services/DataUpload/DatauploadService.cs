using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.DataUpload.Extensions;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.DataUpload.Models;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.DataUpload;

public interface IDataUploadService
{
    Task<List<string>> UploadToApi(string organisationId, BufferedSingleFileUploadDb fileUpload, bool useSpreadsheetServiceId = false);
}

public class DataUploadService : IDataUploadService
{
    private const long ID_NOT_SET = 0;

    private readonly IOrganisationAdminClientService _organisationAdminClientService;
    private readonly IPostcodeLocationClientService _postcodeLocationClientService;
    private readonly ILogger<DataUploadService> _logger;
    private bool _useSpreadsheetServiceId = true;
    private List<OrganisationDto> _organisations = new();
    private readonly List<OrganisationWithServicesDto> _organisationsWithServices = new();
    private readonly List<TaxonomyDto> _taxonomies = new();
    private readonly List<string> _errors = new List<string>();
    private readonly List<ContactDto> _contacts = new();
    private readonly IExcelReader _excelReader;

    public DataUploadService(
        ILogger<DataUploadService> logger,
        IOrganisationAdminClientService organisationAdminClientService,
        IPostcodeLocationClientService postcodeLocationClientService,
        IExcelReader excelReader)
    {
        _organisationAdminClientService = organisationAdminClientService;
        _postcodeLocationClientService = postcodeLocationClientService;
        _excelReader = excelReader;
        _logger = logger;
    }

    public async Task<List<string>> UploadToApi(string organisationId, BufferedSingleFileUploadDb fileUpload, bool useSpreadsheetServiceId = false)
    {
        _logger.LogInformation($"UploadToApi Started for file - {fileUpload.FormFile.FileName}");

        _useSpreadsheetServiceId = useSpreadsheetServiceId;
        var taxonomies = await _organisationAdminClientService.GetTaxonomyList(1, 999999999);
        _taxonomies.AddRange(taxonomies.Items);

        List<DataUploadRowDto> uploadData;

        try
        {
            uploadData = await _excelReader.GetRequestsDataFromExcel(fileUpload);
        }
        catch (DataUploadException ex)
        {
            _logger.LogWarning(ex.Message);
            return new List<string> { ex.Message }; // We control these errors so safe to return to UI
        }
        catch (Exception ex)
        {
            _logger.LogError($"GetRequestsDataFromExcel Failed : {ex.Message}");
            return new List<string> { "Failed to read data from excel spreadsheet" };
        }

        var services = await ProcessRows(uploadData);

        foreach (var service in services)
        {
            try
            {
                await _organisationAdminClientService.CreateService(service.Service!);//TODO create and update
            }
            catch (ApiException ex)
            {
                foreach(var error in ex.ApiErrorResponse.Errors)
                {
                    _errors.Add($"Service {service.Service!.ServiceOwnerReferenceId} failed to be created - {error.PropertyName}:{error.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                var msg = $"Service {service.Service!.ServiceOwnerReferenceId} failed to be created";
                _logger.LogError(ex, msg);
                _errors.Add(msg);
            }
        }

        _logger.LogInformation($"UploadToApi completed with {_errors.Count} errors for file - {fileUpload.FormFile.FileName}");
        return _errors;
    }

    private async Task<List<ServiceForUpload>> ProcessRows(List<DataUploadRowDto> uploadData)
    {
        var servicesForUpload = new List<ServiceForUpload>();

        //  Iterate Organisations
        foreach (var organisationGroupedData in uploadData.GroupBy(p => p.LocalAuthority))
        {
            var organisation = await GetOrganisation(organisationGroupedData.Key);

            if (organisation is null)
            {
                var rows = organisationGroupedData.Select(m => m.ExcelRowId).ToList();
                rows.ForEach(r => _errors.Add($"Failed to find local authority row:{r}"));
                continue;
            }

            foreach (var serviceGroupedData in organisationGroupedData.GroupBy(p => p.ServiceOwnerReferenceId))
            {
                try
                {
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
            RelatedRows = dataUploadRowDtos.Select(m => m.ExcelRowId).ToList()
        };

        var existingService = organisation.Services.Where(x => x.ServiceOwnerReferenceId == dataUploadRowDtos.First().ServiceOwnerReferenceId).FirstOrDefault();

        var service = new ServiceDto
        {
            Id = existingService?.Id ?? ID_NOT_SET,
            OrganisationId = organisation.Id,
            ServiceOwnerReferenceId = serviceGroupedData.Key,
            ServiceType = GetServiceType(organisation.OrganisationType),
            Name = dataUploadRowDtos.GetServiceValue(x => x.NameOfService),
            Description = dataUploadRowDtos.GetServiceValue(x => x.ServiceDescription),
            Status = ServiceStatusType.Active,
            DeliverableType = DeliverableType.NotSet,
            AttendingType = AttendingType.NotSet,
        };


        foreach (var serviceRow in dataUploadRowDtos)
        {
            serviceRow.UpdateServiceDeliveries(service);
            await serviceRow.UpdateLocations(existingService, service, _postcodeLocationClientService);
           // serviceRow.UpdateContacts(existingService, service);
        }

        serviceForUpload.Service = service;
        return serviceForUpload;
    }

    private async Task<OrganisationWithServicesDto?> GetOrganisation(string organisationName)
    {
        _logger.LogInformation($"Getting OrganisationWithServicesDto for Organisation {organisationName}");
        if (!_organisations.Any() || _organisations.Count(x => x.Name == organisationName) == 0)
        {
            _organisations = await _organisationAdminClientService.GetListOrganisations();
        }

        var organisation = _organisations.FirstOrDefault(x => string.Equals(x.Name, organisationName, StringComparison.InvariantCultureIgnoreCase));
        if (organisation == null)
        {
            _logger.LogInformation($"Organisation '{organisationName}' does not exist");
            return null;
        }

        var organisationWithServices = _organisationsWithServices.FirstOrDefault(o => o.Id == organisation.Id);

        if (organisationWithServices is null || organisationWithServices.Services is { Count: >= 0 })
        {
            organisationWithServices = await _organisationAdminClientService.GetOrganisationById(organisation.Id);

            _organisationsWithServices.Add(organisationWithServices!);
        }

        organisationWithServices!.AdminAreaCode = organisation.AdminAreaCode;

        _logger.LogInformation($"Returning OrganisationWithServicesDto '{organisationName}'");
        return organisationWithServices;
    }

    private static ServiceType GetServiceType(OrganisationType organisationType)
    {
        if (organisationType == OrganisationType.LA)
        {
            return ServiceType.FamilyExperience;
        }
        return ServiceType.InformationSharing;
    }

}