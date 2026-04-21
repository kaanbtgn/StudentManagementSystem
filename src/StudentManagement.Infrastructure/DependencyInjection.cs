using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using StudentManagement.Application.Interfaces;
using StudentManagement.Domain.Repositories;
using StudentManagement.Infrastructure.Cache;
using StudentManagement.Infrastructure.ExternalServices;
using StudentManagement.Infrastructure.Helpers;
using StudentManagement.Infrastructure.Audit;
using StudentManagement.Infrastructure.MongoDB;
using StudentManagement.Infrastructure.Persistence;
using StudentManagement.Infrastructure.Persistence.Repositories;
using StudentManagement.Infrastructure.Realtime;

namespace StudentManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // AuditInterceptor — Singleton; DbContext Scoped olduğundan sp üzerinden çözülür
        services.AddHttpContextAccessor();
        services.AddSingleton<AuditInterceptor>(sp =>
            new AuditInterceptor(
                sp.GetRequiredService<MongoDbContext>().AuditLogs,
                sp.GetRequiredService<IHttpContextAccessor>()));

        // PostgreSQL / EF Core
        services.AddDbContext<StudentDbContext>((sp, opt) =>
            opt.UseNpgsql(configuration.GetConnectionString("Postgres"))
               .AddInterceptors(sp.GetRequiredService<AuditInterceptor>()));

        // Redis — ConnectionMultiplexer Singleton lifecyle ile kaydedilir
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(
                configuration.GetConnectionString("Redis")
                ?? throw new InvalidOperationException("ConnectionStrings:Redis is not configured.")));

        // MongoDB (iş veritabanı)
        services.AddSingleton<MongoDbContext>();

        // IDistributedCache — mevcut multiplexer'ı yeniden kullanır (ikinci bağlantı açılmaz)
        services.AddSingleton<IDistributedCache>(sp =>
        {
            var mux = sp.GetRequiredService<IConnectionMultiplexer>();
            return new RedisCache(new RedisCacheOptions
            {
                ConnectionMultiplexerFactory = () => Task.FromResult(mux),
            });
        });

        // Belge cache servisi — 30 dk TTL ile dosyaları saklar
        services.AddScoped<IDocumentCacheService, DocumentCacheService>();

        // Repositories
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IExamGradeRepository, ExamGradeRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Fuzzy matching — Agent OCR çıktısından çıkardığı ismi DB adaylarına eşleştirir
        services.AddSingleton<IFuzzyMatcher, FuzzyMatcher>();

        // Agent HTTP Client + session history (Infrastructure sahipleniyor)
        services.AddSingleton<ChatSessionHistoryService>();
        services.AddSingleton<ISessionHistoryService>(sp => sp.GetRequiredService<ChatSessionHistoryService>());

        string agentBaseUrl = configuration["Agent:BaseUrl"]
            ?? throw new InvalidOperationException("Agent:BaseUrl is not configured.");

        services.AddHttpClient<IAgentClient, AgentHttpClient>(client =>
            client.BaseAddress = new Uri(agentBaseUrl));

        // SignalR + Realtime bildirim servisi
        services.AddSignalR();
        services.AddScoped<IRealtimeNotificationService, SignalRNotificationService>();

        return services;
    }
}
