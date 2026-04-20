using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using StudentManagement.Infrastructure.MongoDB;

namespace StudentManagement.Infrastructure;

/// <summary>
/// Uygulama pipeline'ına Infrastructure startup adımlarını ekleyen extension'lar.
/// Program.cs'i temiz tutar; her adım kendi sınıfında yönetilir.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// MongoDB AuditLogs koleksiyonu için gerekli index'lerin mevcut olmasını sağlar.
    /// İdempotent — index zaten varsa MongoDB sessizce atlar.
    /// </summary>
    public static async Task EnsureAuditIndexes(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var mongo = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
        await mongo.EnsureIndexesAsync();
        await MongoIndexInitializer.EnsureAuditIndexesAsync(mongo.AuditLogs);
    }
}
