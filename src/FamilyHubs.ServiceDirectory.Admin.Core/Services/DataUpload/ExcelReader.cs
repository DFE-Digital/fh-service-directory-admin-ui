using System.Globalization;
using FamilyHubs.ServiceDirectory.Admin.Core.Constants;
using FamilyHubs.ServiceDirectory.Admin.Core.Exceptions;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using NPOI.HSSF.UserModel;
using NPOI.OpenXml4Net.OPC;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace FamilyHubs.ServiceDirectory.Admin.Core.Services.DataUpload;

public interface IExcelReader
{
    public Task<List<DataUploadRowDto>> GetRequestsDataFromExcel(BufferedSingleFileUploadDb fileUpload);
}

public class ExcelReader : IExcelReader
{
    private static async Task<ISheet?> GetFileStream(BufferedSingleFileUploadDb fileUpload)
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

    private static void ThrowIfInvalidateColumnHeaders(ISheet sheet)
    {
        var headerRow = sheet.GetRow(0);
        var errors = new List<string>();

        ValidateHeader(headerRow, ColumnIndex.ServiceUniqueIdentifier, ColumnHeaders.ServiceUniqueIdentifier, errors);
        ValidateHeader(headerRow, ColumnIndex.LocalAuthority, ColumnHeaders.LocalAuthority, errors);
        ValidateHeader(headerRow, ColumnIndex.OrganisationType, ColumnHeaders.OrganisationType, errors);
        ValidateHeader(headerRow, ColumnIndex.NameOfOrganisation, ColumnHeaders.NameOfOrganisation, errors);
        ValidateHeader(headerRow, ColumnIndex.NameOfService, ColumnHeaders.NameOfService, errors);
        ValidateHeader(headerRow, ColumnIndex.DeliveryMethod, ColumnHeaders.DeliveryMethod, errors);
        ValidateHeader(headerRow, ColumnIndex.LocationName, ColumnHeaders.LocationName, errors);
        ValidateHeader(headerRow, ColumnIndex.LocationDescription, ColumnHeaders.LocationDescription, errors);
        ValidateHeader(headerRow, ColumnIndex.AddressLineOne, ColumnHeaders.AddressLineOne, errors);
        ValidateHeader(headerRow, ColumnIndex.AddressLineTwo, ColumnHeaders.AddressLineTwo, errors);
        ValidateHeader(headerRow, ColumnIndex.TownOrCity, ColumnHeaders.TownOrCity, errors);
        ValidateHeader(headerRow, ColumnIndex.County, ColumnHeaders.County, errors);
        ValidateHeader(headerRow, ColumnIndex.Postcode, ColumnHeaders.Postcode, errors);
        ValidateHeader(headerRow, ColumnIndex.ContactEmail, ColumnHeaders.ContactEmail, errors);
        ValidateHeader(headerRow, ColumnIndex.ContactPhone, ColumnHeaders.ContactPhone, errors);
        ValidateHeader(headerRow, ColumnIndex.Website, ColumnHeaders.Website, errors);
        ValidateHeader(headerRow, ColumnIndex.ContactSms, ColumnHeaders.ContactSms, errors);
        ValidateHeader(headerRow, ColumnIndex.SubCategory, ColumnHeaders.SubCategory, errors);
        ValidateHeader(headerRow, ColumnIndex.CostInPounds, ColumnHeaders.CostInPounds, errors);
        ValidateHeader(headerRow, ColumnIndex.CostPer, ColumnHeaders.CostPer, errors);
        ValidateHeader(headerRow, ColumnIndex.CostDescription, ColumnHeaders.CostDescription, errors);
        ValidateHeader(headerRow, ColumnIndex.Language, ColumnHeaders.Language, errors);
        ValidateHeader(headerRow, ColumnIndex.AgeFrom, ColumnHeaders.AgeFrom, errors);
        ValidateHeader(headerRow, ColumnIndex.AgeTo, ColumnHeaders.AgeTo, errors);
        ValidateHeader(headerRow, ColumnIndex.OpeningHoursDescription, ColumnHeaders.OpeningHoursDescription, errors);
        ValidateHeader(headerRow, ColumnIndex.MoreDetailsServiceDescription, ColumnHeaders.MoreDetailsServiceDescription, errors);

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

        return cell.CellType switch
        {
            CellType.Numeric => DateUtil.IsCellDateFormatted(cell)
                ? cell.DateCellValue.ToString(CultureInfo.InvariantCulture)
                : cell.NumericCellValue.ToString(CultureInfo.InvariantCulture),
            CellType.String => cell.StringCellValue,
            _ => string.Empty
        };
    }

    private static OrganisationType GetOrganisationTypeFromCell(IRow row, int column)
    {
        var cell = GetCell(row, column);

        if(string.IsNullOrEmpty(cell))
            return OrganisationType.NotSet;

        return cell switch
        {
            "Local Authority" => OrganisationType.LA,
            "Voluntary and Community Sector" => OrganisationType.VCFS,
            _ => OrganisationType.Company
        };
    }

    private static ServiceDeliveryType GetServiceDeliveryTypeFromCell(IRow row, int column)
    {
        var cell = GetCell(row, column);

        if (string.IsNullOrWhiteSpace(cell))
            return ServiceDeliveryType.NotSet;

        return cell switch
        {
            "In person" => ServiceDeliveryType.InPerson,
            "Online" => ServiceDeliveryType.Online,
            "Telephone" => ServiceDeliveryType.Telephone,
            _ => ServiceDeliveryType.NotSet
        };
    }

    private static void ValidateHeader(IRow row, int columnIndex, string expectedHeader, ICollection<string> listErrors)
    {
        var actualValue = row.GetCell(columnIndex).ToString();

        if (expectedHeader == actualValue) return;
        
        var excelColumnLetter = (char)(65 + columnIndex);
        listErrors.Add($"Column {excelColumnLetter} should be '{expectedHeader}' but is '{actualValue}'");
    }

    private static bool IsRowPopulated(IRow spreadsheetRow)
    {

        for (var i = ColumnIndex.FirstColumn; i <= ColumnIndex.LastColumn; i++)
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
            ServiceOwnerReferenceId = GetCell(spreadsheetRow, ColumnIndex.ServiceUniqueIdentifier),
            LocalAuthority = GetCell(spreadsheetRow, ColumnIndex.LocalAuthority),
            OrganisationType = GetOrganisationTypeFromCell(spreadsheetRow, ColumnIndex.OrganisationType),
            NameOfOrganisation = GetCell(spreadsheetRow, ColumnIndex.NameOfOrganisation),
            NameOfService = GetCell(spreadsheetRow, ColumnIndex.NameOfService),
            DeliveryMethod = GetServiceDeliveryTypeFromCell(spreadsheetRow, ColumnIndex.DeliveryMethod),
            LocationName = GetCell(spreadsheetRow, ColumnIndex.LocationName),
            LocationDescription = GetCell(spreadsheetRow, ColumnIndex.LocationDescription),
            AddressLineOne = GetCell(spreadsheetRow, ColumnIndex.AddressLineOne),
            AddressLineTwo = GetCell(spreadsheetRow, ColumnIndex.AddressLineTwo),
            TownOrCity = GetCell(spreadsheetRow, ColumnIndex.TownOrCity),
            County = GetCell(spreadsheetRow, ColumnIndex.County),
            Postcode = GetCell(spreadsheetRow, ColumnIndex.Postcode),
            ContactEmail = GetCell(spreadsheetRow, ColumnIndex.ContactEmail),
            ContactPhone = GetCell(spreadsheetRow, ColumnIndex.ContactPhone),
            Website = GetCell(spreadsheetRow, ColumnIndex.Website),
            ContactSms = GetCell(spreadsheetRow, ColumnIndex.ContactSms),
            SubCategory = GetCell(spreadsheetRow, ColumnIndex.SubCategory),
            CostInPounds = GetCell(spreadsheetRow, ColumnIndex.CostInPounds),
            CostPer = GetCell(spreadsheetRow, ColumnIndex.CostPer),
            CostDescription = GetCell(spreadsheetRow, ColumnIndex.CostDescription),
            Language = GetCell(spreadsheetRow, ColumnIndex.Language),
            AgeFrom = GetCell(spreadsheetRow, ColumnIndex.AgeFrom),
            AgeTo = GetCell(spreadsheetRow, ColumnIndex.AgeTo),
            OpeningHoursDescription = GetCell(spreadsheetRow, ColumnIndex.OpeningHoursDescription),
            ServiceDescription = GetCell(spreadsheetRow, ColumnIndex.MoreDetailsServiceDescription)
        };
        return dataUploadRow;
    }
}
