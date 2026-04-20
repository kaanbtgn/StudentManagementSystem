using System.ComponentModel;

namespace StudentManagement.MCP.Models;

/// <summary>
/// Word / PDF belgelerindeki tablo verisi.
/// </summary>
public record DocumentTable(
    [Description("Sütun başlıkları")]
    IReadOnlyList<string> Headers,

    [Description("Tablo satırları — her satır, başlıklarla eşleşen hücre değerlerinden oluşan bir dizi")]
    IReadOnlyList<IReadOnlyList<string>> Rows
);

/// <summary>
/// Word / PDF belgelerindeki tek bir bölüm.
/// </summary>
public record DocumentSection(
    [Description("Bölüm başlığı")]
    string Heading,

    [Description("Bölüm gövde metni (isteğe bağlı)")]
    string? Body,

    [Description("Bölüme ait tablolar (isteğe bağlı)")]
    IReadOnlyList<DocumentTable>? Tables = null
);

/// <summary>
/// Word ve PDF çıktısı için üst düzey belge içeriği.
/// Agent bu modeli JSON olarak doldurup generate_document tool'una iletir.
/// </summary>
public record StandardDocumentContent(
    [Description("Belgenin ana başlığı")]
    string Title,

    [Description("Belgeyi oluşturan alt bölümler")]
    IReadOnlyList<DocumentSection> Sections
);
