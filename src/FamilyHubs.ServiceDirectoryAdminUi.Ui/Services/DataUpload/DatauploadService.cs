using FamilyHubs.ServiceDirectory.Shared.Builders;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.DataUpload.Helpers;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.DataUpload;

public interface IDataUploadService
{
    Task<List<string>> UploadToApi(string organisationId, BufferedSingleFileUploadDb fileUpload, bool useSpreadsheetServiceId = false);
}

public class DataUploadService : IDataUploadService
{
    private readonly IOrganisationAdminClientService _organisationAdminClientService;
    private readonly IPostcodeLocationClientService _postcodeLocationClientService;
    private readonly ILogger<DataUploadService> _logger;

    private bool _useSpreadsheetServiceId = true;
    private List<OrganisationDto> _organisations = new();
    private readonly List<OrganisationWithServicesDto> _organisationsWithServices = new();
    private readonly List<TaxonomyDto> _taxonomies = new();
    private readonly List<string> _errors = new List<string>();
    private readonly Dictionary<string, PostcodesIoResponse> _postCodesCache = new Dictionary<string, PostcodesIoResponse>();
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

        List<DataUploadRow> uploadData;

        try
        {
            uploadData = await _excelReader.GetRequestsDataFromExcel(fileUpload);
        }
        catch(DataUploadException ex)
        {
            _logger.LogWarning(ex.Message);
            return new List<string> { ex.Message }; // We control these errors so safe to return to UI
        }
        catch(Exception ex) 
        {
            _logger.LogError($"GetRequestsDataFromExcel Failed : {ex.Message}");
            return new List<string> { "Failed to read data from excel spreadsheet" };
        }
        
        await ProcessRows(uploadData);

        _logger.LogInformation($"UploadToApi completed with {_errors.Count} errors for file - {fileUpload.FormFile.FileName}");
        return _errors;
    }

    private async Task ProcessRows(List<DataUploadRow> uploadData)
    {

        foreach (var dtRow in uploadData)
        {
            _logger.LogInformation($"Processing row {dtRow.ExcelRowId}");

            var localAuthority = await GetOrganisationsWithOutServices(dtRow.LocalAuthority!);
            if (localAuthority == null)
            {
                RecordWarning($"Failed to find local authority row:{dtRow.ExcelRowId}");
                continue;
            }

            if(!OrganisationHelper.TryResolveOrganisationType(dtRow, out OrganisationTypeDto organisationTypeDto, out string? organisationName))
            {
                RecordWarning($"Name of organisation missing row:{dtRow.ExcelRowId}");
                continue;
            }

            var newOrganisation = false;
            OrganisationWithServicesDto? organisationDto;
            if (organisationTypeDto.Name == "LA" || organisationTypeDto.Name == "FamilyHub")
            {
                organisationDto = await GetOrganisation(dtRow.LocalAuthority!);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(organisationName))
                {
                    RecordWarning($"Name of organisation missing row:{dtRow.ExcelRowId}");
                    continue;
                }
                organisationDto = await GetOrganisation(organisationName);
                if (organisationDto == null)
                {
                    organisationDto = new OrganisationWithServicesDto
                    (
                        Guid.NewGuid().ToString(),
                        organisationTypeDto,
                        organisationName,
                        organisationName,
                        null,
                        dtRow.Website,
                        dtRow.Website
                    )
                    {
                        AdminAreaCode = localAuthority.AdminAreaCode
                    };

                    newOrganisation = true;
                }
            }

            _contacts.AddRange(ContactHelper.GetAllContactsFromOrganisation(organisationDto));

            if (newOrganisation)
            {
                var service = await GetServiceFromRow(dtRow.ExcelRowId, dtRow, null, organisationTypeDto, organisationDto?.Id ?? string.Empty);
                if (organisationDto != null && service != null)
                {
                    organisationDto.Services = new List<ServiceDto>
                    {
                        service
                    };

                    try
                    {
                        //Create Organisation
                        var _ = await _organisationAdminClientService.CreateOrganisation(organisationDto);
                        _logger.LogInformation($"New organisation created {organisationDto.Name}");
                    }
                    catch(Exception exp)
                    {
                        _logger.LogError($"Failed to create Organisation {organisationDto.Name} : {exp.Message}");
                        _errors.Add($"Failed to create organisation with service row:{dtRow.ExcelRowId}");
                    }

                }
            }
            else
            {
                var isNewService = true;
                ServiceDto? service;
                if (_useSpreadsheetServiceId)
                {
                    if (string.IsNullOrEmpty(dtRow.ServiceUniqueId))
                    {
                        RecordWarning($"Service unique identifier missing row:{dtRow.ExcelRowId}");
                        continue;
                    }

                    service = organisationDto?.Services?.FirstOrDefault(x => x.Id == $"{organisationDto.AdminAreaCode?.Remove(0, 1)}{dtRow.ServiceUniqueId}");

                }
                else
                {
                    service = organisationDto?.Services?.FirstOrDefault(x => x.Name == dtRow.NameOfService);
                }

                if (service != null)
                {
                    isNewService = false;
                }
                service = await GetServiceFromRow(dtRow.ExcelRowId, dtRow, service, organisationTypeDto, organisationDto?.Id ?? string.Empty);

                if (isNewService)
                {
                    if (service != null)
                    {
                        try
                        {
                            var _ = await _organisationAdminClientService.CreateService(service);
                            _logger.LogInformation($"New service created {service.Name}");
                        }
                        catch(Exception exp)
                        {
                            _logger.LogError("Failed to create service :{errorMessage} {stackTrace}", exp.Message, exp.StackTrace);
                            _errors.Add($"Failed to create service row:{dtRow.ExcelRowId}");
                        }

                    }
                }
                else
                {
                    if (service != null)
                    {
                        try
                        {
                            var _ = await _organisationAdminClientService.UpdateService(service);
                            _logger.LogInformation($"Service updated {service.Name}");
                        }
                        catch (Exception exp)
                        {
                            _logger.LogError("Failed to update service :{errorMessage} {stackTrace}", exp.Message, exp.StackTrace);
                            _errors.Add($"Failed to update service row:{dtRow.ExcelRowId}");
                        }

                    }
                }
            }
        }
    }

    private async Task<ServiceDto?> GetServiceFromRow(int rowNumber, DataUploadRow dtRow, ServiceDto? service, OrganisationTypeDto organisationTypeDto, string organisationId)
    {
        var description = dtRow.ServiceDescription;

        var locations = await LocationsHelper.GetServiceAtLocations(dtRow, service, _errors, _contacts,_taxonomies,_postCodesCache,_postcodeLocationClientService);
        
        var serviceId = service?.Id ?? Guid.NewGuid().ToString();
        if (string.IsNullOrEmpty(dtRow.ServiceUniqueId))
        {
            _errors.Add($"Service unique identifier missing row:{rowNumber}");
            return null;
        }
        if (service == null && _useSpreadsheetServiceId && !string.IsNullOrEmpty(dtRow.ServiceUniqueId))
        {
            var organisation = await GetOrganisationsWithOutServices(dtRow.LocalAuthority ?? string.Empty);
            serviceId = organisation is not null ?
            $"{organisation.AdminAreaCode?.Remove(0, 1)}{dtRow.ServiceUniqueId}" : Guid.NewGuid().ToString();
        }

        var builder = new ServicesDtoBuilder();
        var result = builder.WithMainProperties(serviceId,
                                   ServiceHelper.GetServiceType(organisationTypeDto),
                                   organisationId,
                                   dtRow.NameOfService!,
                                   description,
                                   null,
                                   null,
                                   null,
                                   dtRow.DeliveryMethod,
                                   dtRow.DeliveryMethod,
                                   "active",
                                   string.Empty,
                                   false)
                        .WithServiceDelivery(ServiceHelper.GetDeliveryTypes(dtRow.DeliveryMethod ?? string.Empty, service))
                        .WithServiceAtLocations(locations)
                        .WithLinkContact(ContactHelper.GetLinkContacts(serviceId, LinkContactTypes.SERVICE, dtRow, service?.LinkContacts, _contacts, _errors))
                        .WithCostOption(ServiceHelper.GetCosts(dtRow, service))
                        .WithLanguages(ServiceHelper.GetLanguages(dtRow, service))
                        .WithServiceTaxonomies(GetTaxonomies(dtRow, service))
                        .WithEligibility(ServiceHelper.GetEligibilities(dtRow, service))
                        .Build();

        return result;

    }

    private List<ServiceTaxonomyDto> GetTaxonomies(DataUploadRow dtRow, ServiceDto? existingServiceDto)
    {
        List<ServiceTaxonomyDto> list = new();
        var categories = dtRow.SubCategory;
        if (!string.IsNullOrEmpty(categories))
        {
            var parts = categories.Split('|').Select(s => s.Trim());
            foreach (var part in parts)
            {
                var taxonomy = _taxonomies.FirstOrDefault(x => string.Equals(x.Name, part, StringComparison.CurrentCultureIgnoreCase));
                if (taxonomy != null)
                {
                    var existing = existingServiceDto?.ServiceTaxonomies?.SingleOrDefault(t => t.Taxonomy?.Name == taxonomy.Name);
                    var serviceTaxonomyId = existing is not null ? existing.Id : Guid.NewGuid().ToString();
                    
                    list.Add(new ServiceTaxonomyDto(serviceTaxonomyId, taxonomy));
                }
            }
        }
        return list;
    }

    private async Task<OrganisationDto?> GetOrganisationsWithOutServices(string organisationName)
    {
        if (!_organisations.Any() || _organisations.Count(x => x.Name == organisationName) == 0)
        {
            _organisations = await _organisationAdminClientService.GetListOrganisations();
        }

        var organisation = _organisations.FirstOrDefault(x => organisationName.Contains(x.Name ?? string.Empty));
        if (organisation == null)
        {
            return null;
        }
        return organisation;
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

            _organisationsWithServices.Add(organisationWithServices);
        }

        organisationWithServices.AdminAreaCode = organisation.AdminAreaCode;

        _logger.LogInformation($"Returning OrganisationWithServicesDto '{organisationName}'");
        return organisationWithServices;
    }

    private void RecordWarning(string message)
    {
        _logger.LogWarning(message);
        _errors.Add(message);
    }
}