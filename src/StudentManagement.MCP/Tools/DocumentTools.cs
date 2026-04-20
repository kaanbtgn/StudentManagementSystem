using System.ComponentModel;
using System.Net.Http.Json;
using ModelContextProtocol.Server;

namespace StudentManagement.MCP.Tools;

[McpServerToolType]
public sealed class DocumentTools
{
    private readonly HttpClient _http;

    public DocumentTools(IHttpClientFactory factory)
        => _http = factory.CreateClient("StudentManagementApi");

    /// <summary>
    /// FuzzyMatchStudents ile aynı işi yapar; bu tool StudentTools.FuzzyMatchStudents'a
    /// yönlendirme için bırakılmıştır. Yeni tool'larda StudentTools kullanılmalıdır.
    /// </summary>
    [McpServerTool]
    [Description(
        "OCR'dan çıkarılan (veya kullanıcının yazdığı) öğrenci adını, " +
        "veritabanındaki gerçek adlarla bulanık (fuzzy) eşleştirir. " +
        "candidates listesi SearchStudents ile alınan aday ad listesidir. " +
        "requiresConfirmation true dönerse kullanıcıdan onay al.")]
    public async Task<string> FuzzyMatchStudent(
        [Description("Eşleştirilecek öğrenci adı (OCR çıktısından veya kullanıcı girdisinden)")]
        string query,
        [Description("Veritabanından alınan aday öğrenci adları listesi")]
        IEnumerable<string> candidates,
        [Description("Minimum benzerlik eşiği, 0.0–1.0 arasında. Varsayılan: 0.75")]
        double threshold = 0.75,
        CancellationToken ct = default)
    {
        var body = new { query, candidates, threshold };
        var response = await _http.PostAsJsonAsync("api/students/fuzzy-match", body, ct);
        return await response.Content.ReadAsStringAsync(ct);
    }
}


