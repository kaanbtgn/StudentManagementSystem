using System.ComponentModel;
using ModelContextProtocol.Server;

namespace StudentManagement.MCP.Tools;

[McpServerToolType]
public sealed class DocumentTools
{
    private readonly HttpClient _http;

    public DocumentTools(IHttpClientFactory factory)
        => _http = factory.CreateClient("StudentManagementApi");

    /// <summary>
    /// StudentTools.FuzzyMatchStudents ile aynı işi yapar; geriye dönük uyumluluk için korunmuştur.
    /// </summary>
    [McpServerTool]
    [Description(
        "OCR'dan çıkarılan (veya kullanıcının yazdığı) öğrenci adını " +
        "veritabanında pg_trgm similarity ile doğrudan arar. " +
        "candidates listesi gerekmez; eşleşme DB tarafında GIN indeksiyle yapılır. " +
        "requiresConfirmation true dönerse kullanıcıdan onay al.")]
    public async Task<string> FuzzyMatchStudent(
        [Description("Eşleştirilecek öğrenci adı (OCR çıktısından veya kullanıcı girdisinden)")]
        string query,
        [Description("Minimum benzerlik eşiği, 0.0–1.0 arasında. Varsayılan: 0.3")]
        double threshold = 0.3,
        CancellationToken ct = default)
    {
        var response = await _http.GetAsync(
            $"api/students/fuzzy-search?q={Uri.EscapeDataString(query)}&threshold={threshold}", ct);
        return await response.Content.ReadAsStringAsync(ct);
    }
}



