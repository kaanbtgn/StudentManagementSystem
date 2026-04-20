using System.ComponentModel;
using System.Net.Http.Json;
using ModelContextProtocol.Server;

namespace StudentManagement.MCP.Tools;

[McpServerToolType]
public sealed class AuditTools
{
    private readonly HttpClient _http;

    public AuditTools(IHttpClientFactory factory)
        => _http = factory.CreateClient("StudentManagementApi");

    [McpServerTool]
    [Description(
        "Belirtilen öğrenciye ait KVKK denetim geçmişini listeler (son 50 kayıt). " +
        "Her kayıtta işlem türü (Create/Update/Delete), zaman damgası ve " +
        "değişen alanların snapshot'ı yer alır. " +
        "Hassas alanlar (Email, Phone) zaten [MASKED] olarak saklanmıştır.")]
    public async Task<string> GetStudentAuditHistory(
        [Description("Denetim geçmişi sorgulanacak öğrencinin benzersiz kimliği (UUID)")]
        Guid studentId,
        CancellationToken ct)
    {
        var response = await _http.GetAsync($"api/audit/students/{studentId}", ct);
        return await response.Content.ReadAsStringAsync(ct);
    }
}
