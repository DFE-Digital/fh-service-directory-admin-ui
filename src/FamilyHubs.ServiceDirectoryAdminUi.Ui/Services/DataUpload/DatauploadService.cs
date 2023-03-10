using FamilyHubs.ServiceDirectory.Shared.Builders;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;

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

        List<DataUploadRowDto> uploadData;

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

    private void RecordWarning(string message)
    {
        _logger.LogWarning(message);
        _errors.Add(message);
    }
}