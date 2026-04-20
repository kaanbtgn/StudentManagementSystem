using ClosedXML.Excel;
using StudentManagement.MCP.Models;

namespace StudentManagement.MCP.Services;

internal sealed class ExcelGenerator : IExcelGenerator
{
    public (byte[] Content, string FileName, string ContentType) Generate(ExcelDocumentContent content)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add(content.SheetName);

        // Başlık satırı — koyu, zemin renkli
        for (int col = 0; col < content.Headers.Count; col++)
        {
            var cell = sheet.Cell(1, col + 1);
            cell.Value = content.Headers[col];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightSteelBlue;
        }

        // Veri satırları
        for (int row = 0; row < content.Rows.Count; row++)
        {
            var rowData = content.Rows[row];
            for (int col = 0; col < rowData.Count; col++)
                sheet.Cell(row + 2, col + 1).Value = rowData[col];
        }

        // Sütun genişliklerini içeriğe göre otomatik ayarla
        sheet.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);

        var fileName = $"{Slugify(content.SheetName)}.xlsx";
        return (ms.ToArray(), fileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    private static string Slugify(string name)
        => string.Concat(name.Split(Path.GetInvalidFileNameChars()))
                 .Replace(' ', '_')
                 .ToLowerInvariant();
}
