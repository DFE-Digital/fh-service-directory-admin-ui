using System.Globalization;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;
using NPOI.HSSF.UserModel;
using NPOI.OpenXml4Net.OPC;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.DataUpload;

public interface IExcelReader
{
    public Task<List<DataUploadRowDto>> GetRequestsDataFromExcel(BufferedSingleFileUploadDb fileUpload);
}

internal class ExcelReader : IExcelReader
{
    public async Task<ISheet?> GetFileStream(BufferedSingleFileUploadDb fileUpload)
    {
        var fileExtension = Path.GetExtension(fileUpload.FormFile.FileName);

        string sheetName;
        ISheet? sheet = null;
        using var memoryStream = new MemoryStream();
        await fileUpload.FormFile.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        
        switch (fileExtension)
        {
            case ".xlsx":
            case ".xlsm":
                {

                    var opcPackage = OPCPackage.Open(memoryStream);
                    var wb = new XSSFWorkbook(opcPackage);
                    sheetName = wb.GetSheetAt(0).SheetName;
                    sheet = (XSSFSheet)wb.GetSheet(sheetName);

                }
                break;
            case ".xls":
                {
                    var wb = new HSSFWorkbook(memoryStream);
                    sheetName = wb.GetSheetAt(0).SheetName;
                    sheet = (HSSFSheet)wb.GetSheet(sheetName);
                }
                break;
        }

        return sheet;
    }

    public async Task<List<DataUploadRowDto>> GetRequestsDataFromExcel(BufferedSingleFileUploadDb? fileUpload)
    {
        if(fileUpload == null)
        {
            throw new DataUploadException("No file uploaded");
        }

        var sheet = await GetFileStream(fileUpload);
        var dtExcelTable = new List<DataUploadRowDto>();
        if (sheet == null)
        {
            return dtExcelTable;
        }

        ThrowIfInvalidateColumnHeaders(sheet);

        var i = 5;
        var spreadsheetRow = sheet.GetRow(i);

        while (spreadsheetRow != null)
        {
            i++; //  i is incremented before adding to dictionary because excel spreadsheet is not zero base, this will make it more readable to users

            if (IsRowPopulated(spreadsheetRow))
            {
                var dataUploadRow = GetDataUploadRow(spreadsheetRow, i);
                dtExcelTable.Add(dataUploadRow);
            }
            
            spreadsheetRow = sheet.GetRow(i); // Prep row for next loop
        }

        return dtExcelTable;
    }

    private void ThrowIfInvalidateColumnHeaders(ISheet sheet)
    {
        var headerRow = sheet.GetRow(0);
        var errors = new List<string>();

        ValidateHeader(headerRow, ColumnIndexs.SERVICE_UNIQUE_IDENTIFIER, ColumnHeaders.SERVICE_UNIQUE_IDENTIFIER, errors);
        ValidateHeader(headerRow, ColumnIndexs.LOCAL_AUTHORITY, ColumnHeaders.LOCAL_AUTHORITY, errors);
        ValidateHeader(headerRow, ColumnIndexs.ORGANISATION_TYPE, ColumnHeaders.ORGANISATION_TYPE, errors);
        ValidateHeader(headerRow, ColumnIndexs.NAME_OF_ORGANISATION, ColumnHeaders.NAME_OF_ORGANISATION, errors);
        ValidateHeader(headerRow, ColumnIndexs.NAME_OF_SERVICE, ColumnHeaders.NAME_OF_SERVICE, errors);
        ValidateHeader(headerRow, ColumnIndexs.DELIVERY_METHOD, ColumnHeaders.DELIVERY_METHOD, errors);
        ValidateHeader(headerRow, ColumnIndexs.LOCATION_NAME, ColumnHeaders.LOCATION_NAME, errors);
        ValidateHeader(headerRow, ColumnIndexs.LOCATION_DESCRIPTION, ColumnHeaders.LOCATION_DESCRIPTION, errors);
        ValidateHeader(headerRow, ColumnIndexs.ADDRESS_LINE_ONE, ColumnHeaders.ADDRESS_LINE_ONE, errors);
        ValidateHeader(headerRow, ColumnIndexs.ADDRESS_LINE_TWO, ColumnHeaders.ADDRESS_LINE_TWO, errors);
        ValidateHeader(headerRow, ColumnIndexs.TOWN_OR_CITY, ColumnHeaders.TOWN_OR_CITY, errors);
        ValidateHeader(headerRow, ColumnIndexs.COUNTY, ColumnHeaders.COUNTY, errors);
        ValidateHeader(headerRow, ColumnIndexs.POSTCODE, ColumnHeaders.POSTCODE, errors);
        ValidateHeader(headerRow, ColumnIndexs.CONTACT_EMAIL, ColumnHeaders.CONTACT_EMAIL, errors);
        ValidateHeader(headerRow, ColumnIndexs.CONTACT_PHONE, ColumnHeaders.CONTACT_PHONE, errors);
        ValidateHeader(headerRow, ColumnIndexs.WEBSITE, ColumnHeaders.WEBSITE, errors);
        ValidateHeader(headerRow, ColumnIndexs.CONTACT_SMS, ColumnHeaders.CONTACT_SMS, errors);
        ValidateHeader(headerRow, ColumnIndexs.SUB_CATEGORY, ColumnHeaders.SUB_CATEGORY, errors);
        ValidateHeader(headerRow, ColumnIndexs.COST_IN_POUNDS, ColumnHeaders.COST_IN_POUNDS, errors);
        ValidateHeader(headerRow, ColumnIndexs.COST_PER, ColumnHeaders.COST_PER, errors);
        ValidateHeader(headerRow, ColumnIndexs.COST_DESCRIPTION, ColumnHeaders.COST_DESCRIPTION, errors);
        ValidateHeader(headerRow, ColumnIndexs.LANGUAGE, ColumnHeaders.LANGUAGE, errors);
        ValidateHeader(headerRow, ColumnIndexs.AGE_FROM, ColumnHeaders.AGE_FROM, errors);
        ValidateHeader(headerRow, ColumnIndexs.AGE_TO, ColumnHeaders.AGE_TO, errors);
        ValidateHeader(headerRow, ColumnIndexs.OPENING_HOURS_DESCRIPTION, ColumnHeaders.OPENING_HOURS_DESCRIPTION, errors);
        ValidateHeader(headerRow, ColumnIndexs.MORE_DETAILS_SERVICE_DESCRIPTION, ColumnHeaders.MORE_DETAILS_SERVICE_DESCRIPTION, errors);

        if (errors.Any())
        {
            throw new DataUploadException($"Excel spreadsheet does not match expected format - {string.Join(", ", errors)}");
        }
    }

    private static string GetCell(IRow row, int column)
    {
        var cell = row.GetCell(column);

        if (cell is null)
        {
            return string.Empty;
        }

        switch (cell.CellType)
        {
            case CellType.Numeric:
                return DateUtil.IsCellDateFormatted(cell)
                    ? cell.DateCellValue.ToString(CultureInfo.InvariantCulture)
                    : cell.NumericCellValue.ToString(CultureInfo.InvariantCulture);

            case CellType.String:
                return cell.StringCellValue;

            default:
                return string.Empty;
        }

    }

    private static OrganisationType GetOrganisationTypeFromCell(IRow row, int column)
    {
        var cell = GetCell(row, column);

        if(string.IsNullOrEmpty(cell))
            return OrganisationType.NotSet;

        switch (cell)
        {
            case "Local Authority":
                return OrganisationType.LA;
            case "Voluntary and Community Sector":
                return OrganisationType.VCFS;
            default:
                return OrganisationType.Company;
        }

    }

    private static ServiceDeliveryType GetServiceDeliveryTypeFromCell(IRow row, int column)
    {
        var cell = GetCell(row, column);

        if (string.IsNullOrWhiteSpace(cell))
            return ServiceDeliveryType.NotSet;

        switch (cell)
        {
            case "In person":
                return ServiceDeliveryType.InPerson;
            case "Online":
                return ServiceDeliveryType.Online;
            case "Telephone":
                return ServiceDeliveryType.Telephone;
        }

        return ServiceDeliveryType.NotSet;
    }

    private static void ValidateHeader(IRow row, int columnIndex, string expectedHeader, List<string> listErrors)
    {
        var actualValue = row.GetCell(columnIndex).ToString();

        if (expectedHeader != actualValue)
        {
            var excelColumnLetter = (char)(65 + columnIndex);
            listErrors.Add($"Column {excelColumnLetter} should be '{expectedHeader}' but is '{actualValue}'");
        }
    }

    private static bool IsRowPopulated(IRow spreadsheetRow)
    {

        for (var i = ColumnIndexs.FIRST_COLUMN; i <= ColumnIndexs.LAST_COLUMN; i++)
        {
            var cellValue = GetCell(spreadsheetRow, i);
            if (!string.IsNullOrEmpty(cellValue))
            {
                return true; // At least one cell populated
            }
        }

        return false;
    }

    private static DataUploadRowDto GetDataUploadRow(IRow spreadsheetRow, int rowNumber)
    {
        var dataUploadRow = new DataUploadRowDto
        {
            ExcelRowId = rowNumber,
            ServiceOwnerReferenceId = GetCell(spreadsheetRow, ColumnIndexs.SERVICE_UNIQUE_IDENTIFIER),
            LocalAuthority = GetCell(spreadsheetRow, ColumnIndexs.LOCAL_AUTHORITY),
            OrganisationType = GetOrganisationTypeFromCell(spreadsheetRow, ColumnIndexs.ORGANISATION_TYPE),
            NameOfOrganisation = GetCell(spreadsheetRow, ColumnIndexs.NAME_OF_ORGANISATION),
            NameOfService = GetCell(spreadsheetRow, ColumnIndexs.NAME_OF_SERVICE),
            DeliveryMethod = GetServiceDeliveryTypeFromCell(spreadsheetRow, ColumnIndexs.DELIVERY_METHOD),
            LocationName = GetCell(spreadsheetRow, ColumnIndexs.LOCATION_NAME),
            LocationDescription = GetCell(spreadsheetRow, ColumnIndexs.LOCATION_DESCRIPTION),
            AddressLineOne = GetCell(spreadsheetRow, ColumnIndexs.ADDRESS_LINE_ONE),
            AddressLineTwo = GetCell(spreadsheetRow, ColumnIndexs.ADDRESS_LINE_TWO),
            TownOrCity = GetCell(spreadsheetRow, ColumnIndexs.TOWN_OR_CITY),
            County = GetCell(spreadsheetRow, ColumnIndexs.COUNTY),
            Postcode = GetCell(spreadsheetRow, ColumnIndexs.POSTCODE),
            ContactEmail = GetCell(spreadsheetRow, ColumnIndexs.CONTACT_EMAIL),
            ContactPhone = GetCell(spreadsheetRow, ColumnIndexs.CONTACT_PHONE),
            Website = GetCell(spreadsheetRow, ColumnIndexs.WEBSITE),
            ContactSms = GetCell(spreadsheetRow, ColumnIndexs.CONTACT_SMS),
            SubCategory = GetCell(spreadsheetRow, ColumnIndexs.SUB_CATEGORY),
            CostInPounds = GetCell(spreadsheetRow, ColumnIndexs.COST_IN_POUNDS),
            CostPer = GetCell(spreadsheetRow, ColumnIndexs.COST_PER),
            CostDescription = GetCell(spreadsheetRow, ColumnIndexs.COST_DESCRIPTION),
            Language = GetCell(spreadsheetRow, ColumnIndexs.LANGUAGE),
            AgeFrom = GetCell(spreadsheetRow, ColumnIndexs.AGE_FROM),
            AgeTo = GetCell(spreadsheetRow, ColumnIndexs.AGE_TO),
            OpeningHoursDescription = GetCell(spreadsheetRow, ColumnIndexs.OPENING_HOURS_DESCRIPTION),
            ServiceDescription = GetCell(spreadsheetRow, ColumnIndexs.MORE_DETAILS_SERVICE_DESCRIPTION)
        };
        return dataUploadRow;
    }
}
