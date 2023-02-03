using System.Data;
using System.Globalization;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;
using NPOI.HSSF.UserModel;
using NPOI.OpenXml4Net.OPC;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.DataUpload;

public interface IExcelReader
{
    public Task<ISheet?> GetFileStream(BufferedSingleFileUploadDb fileUpload);

    public Task<DataTable> GetRequestsDataFromExcel(BufferedSingleFileUploadDb fileUpload);
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

    public async Task<DataTable> GetRequestsDataFromExcel(BufferedSingleFileUploadDb fileUpload)
    {
        var sh = await GetFileStream(fileUpload);
        var dtExcelTable = new DataTable();
        if (sh == null)
        {
            return dtExcelTable;
        }
        dtExcelTable.Rows.Clear();
        dtExcelTable.Columns.Clear();
        var headerRow = sh.GetRow(0);
        int colCount = headerRow.LastCellNum;
        for (var c = 0; c < colCount; c++)
            dtExcelTable.Columns.Add(headerRow.GetCell(c).ToString());
        var i = 5;
        var currentRow = sh.GetRow(i);
        while (currentRow != null)
        {
            var dr = dtExcelTable.NewRow();
            var blankCells = 0;
            for (var j = 0; j < colCount; j++)
            {
                var cell = currentRow.GetCell(j);

                if (cell != null)
                    switch (cell.CellType)
                    {
                        case CellType.Numeric:
                            dr[j] = DateUtil.IsCellDateFormatted(cell)
                                ? cell.DateCellValue.ToString(CultureInfo.InvariantCulture)
                                : cell.NumericCellValue.ToString(CultureInfo.InvariantCulture);
                            break;
                        case CellType.String:
                            dr[j] = cell.StringCellValue;
                            break;
                        case CellType.Blank:
                            dr[j] = string.Empty;
                            blankCells++;
                            break;
                    }
            }
            if (blankCells > (currentRow.Cells.Count * 0.75))
            {
                currentRow = null;
                continue;
            }
            dtExcelTable.Rows.Add(dr);
            i++;
            currentRow = sh.GetRow(i);
        }
        return dtExcelTable;
    }
}