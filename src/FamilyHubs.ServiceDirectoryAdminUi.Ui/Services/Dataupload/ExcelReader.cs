using FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;
using NPOI.HSSF.UserModel;
using NPOI.OpenXml4Net.OPC;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Data;
using System.Globalization;
using System.IO;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Dataupload;

internal class ExcelReader
{
    public static async Task<ISheet?> GetFileStream(BufferedSingleFileUploadDb fileUpload)
    {
        var fileExtension = Path.GetExtension(fileUpload.FormFile.FileName);
       
        string sheetName;
        ISheet? sheet = null;
        switch (fileExtension)
        {
            case ".xlsx":
                {
                    
                    using (var memoryStream = new MemoryStream())
                    {
                        await fileUpload.FormFile.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;
                        var opcPackage = OPCPackage.Open(memoryStream);
                        var wb = new XSSFWorkbook(opcPackage);
                        sheetName = wb.GetSheetAt(0).SheetName;
                        sheet = (XSSFSheet)wb.GetSheet(sheetName);
                    }
                    
                }
                break;
            case ".xls":
                using (var memoryStream = new MemoryStream())
                {
                    await fileUpload.FormFile.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;
                    var wb = new HSSFWorkbook(memoryStream);
                    sheetName = wb.GetSheetAt(0).SheetName;
                    sheet = (HSSFSheet)wb.GetSheet(sheetName);
                }
                break;
        }
        return sheet;
    }

    public static async Task<DataTable> GetRequestsDataFromExcel(BufferedSingleFileUploadDb fileUpload)
    {
        try
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
                int blankCells = 0;
                for (var j = 0; j < currentRow.Cells.Count; j++)
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
        catch
        {
            throw;
        }
    }
}
