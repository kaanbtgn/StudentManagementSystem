using System.ComponentModel;
using System.Net.Http.Json;
using ModelContextProtocol.Server;

namespace StudentManagement.MCP.Tools;

[McpServerToolType]
public sealed class StudentTools
{
    private readonly HttpClient _http;

    public StudentTools(IHttpClientFactory factory)
        => _http = factory.CreateClient("StudentManagementApi");

    [McpServerTool]
    [Description("Tüm öğrencileri listeler.")]
    public async Task<string> GetAllStudents(CancellationToken ct)
    {
        var response = await _http.GetAsync("api/students", ct);
        return await response.Content.ReadAsStringAsync(ct);
    }

    [McpServerTool]
    [Description("Öğrenci adı veya soyadına göre arama yapar.")]
    public async Task<string> SearchStudents(
        [Description("Aranacak ad veya soyad terimi")]
        string term,
        CancellationToken ct)
    {
        var response = await _http.GetAsync($"api/students/search?term={Uri.EscapeDataString(term)}", ct);
        return await response.Content.ReadAsStringAsync(ct);
    }

    [McpServerTool]
    [Description(
        "Öğrenci bilgilerini günceller. " +
        "Güncellemeden önce FuzzyMatchStudents ile doğru öğrencinin bulunduğundan emin ol. " +
        "requiresConfirmation true dönmüşse işlemi kullanıcı onaylamadan gerçekleştirme.")]
    public async Task<string> UpdateStudent(
        [Description("Güncellenecek öğrencinin benzersiz kimliği (UUID)")]
        Guid studentId,
        [Description("Yeni ad (değiştirilmeyecekse null bırak)")]
        string? firstName,
        [Description("Yeni soyad (değiştirilmeyecekse null bırak)")]
        string? lastName,
        [Description("Yeni bölüm (değiştirilmeyecekse null bırak)")]
        string? department,
        [Description("Yeni telefon (değiştirilmeyecekse null bırak)")]
        string? phone,
        CancellationToken ct)
    {
        var body = new { firstName, lastName, department, phone };
        var response = await _http.PutAsJsonAsync($"api/students/{studentId}", body, ct);
        return response.IsSuccessStatusCode ? "Güncelleme başarılı." : $"Hata: {response.StatusCode}";
    }

    [McpServerTool]
    [Description(
        "Öğrenci kaydını siler. " +
        "Bu işlem geri alınamaz; kullanıcıdan açık onay aldıktan sonra çağır.")]
    public async Task<string> DeleteStudent(
        [Description("Silinecek öğrencinin benzersiz kimliği (UUID)")]
        Guid studentId,
        CancellationToken ct)
    {
        var response = await _http.DeleteAsync($"api/students/{studentId}", ct);
        return response.IsSuccessStatusCode ? "Silme başarılı." : $"Hata: {response.StatusCode}";
    }

    [McpServerTool]
    [Description(
        "OCR veya kullanıcı girdisinden gelen ismi veritabanı adaylarıyla bulanık eşleştirir. " +
        "Sonuçta requiresConfirmation true ise en iyi eşleşmeyi kullanıcıya göster ve onay iste. " +
        "Onay alınmadan UpdateStudent veya DeleteStudent çağırma.")]
    public async Task<string> FuzzyMatchStudents(
        [Description("Eşleştirilecek öğrenci adı")]
        string query,
        [Description("Aday öğrenci adları listesi (SearchStudents sonucundan alınır)")]
        IEnumerable<string> candidates,
        [Description("Minimum benzerlik eşiği (0.0–1.0). Varsayılan: 0.75")]
        double threshold = 0.75,
        CancellationToken ct = default)
    {
        var body = new { query, candidates, threshold };
        var response = await _http.PostAsJsonAsync("api/students/fuzzy-match", body, ct);
        return await response.Content.ReadAsStringAsync(ct);
    }
}
