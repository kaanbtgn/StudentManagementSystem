using System.ComponentModel;

namespace StudentManagement.MCP.Models;

/// <summary>
/// Excel çıktısı için tek sayfa içeriğini tanımlar.
/// Agent bu modeli JSON olarak doldurup generate_document tool'una iletir.
/// </summary>
public record ExcelDocumentContent(
    [Description("Excel sayfasının adı")]
    string SheetName,

    [Description("Sütun başlıkları dizisi")]
    IReadOnlyList<string> Headers,

    [Description("Satır verileri — her satır, başlıklarla eşleşen hücre değerlerinden oluşan bir dizi")]
    IReadOnlyList<IReadOnlyList<string>> Rows
);
