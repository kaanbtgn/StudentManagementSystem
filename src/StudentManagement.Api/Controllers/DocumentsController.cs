using Microsoft.AspNetCore.Mvc;
using StudentManagement.Api.Models;
using StudentManagement.Application.Interfaces;
using System.Threading.Channels;

namespace StudentManagement.Api.Controllers;

/// <summary>
/// İç endpoint MCP Server, dış endpoint UI tarafından kullanılır.
/// </summary>
[ApiController]
public sealed class DocumentsController : ControllerBase
{
    private readonly IDocumentCacheService _cache;
    private readonly IConfiguration _config;

    public DocumentsController(IDocumentCacheService cache, IConfiguration config)
    {
        _cache = cache;
        _config = config;
    }

    /// <summary>
    /// MCP Server tarafından üretilen belgeyi cache'e yazar.
    /// X-Internal-Api-Key header'ı ile korunur.
    /// </summary>
    [HttpPost("api/internal/docs/store")]
    [DisableRequestSizeLimit]
    [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
    [ApiExplorerSettings(GroupName = "internal")]
    public async Task<IActionResult> StoreAsync(IFormFile file, CancellationToken ct)
    {
        var apiKey = _config["InternalApi:ApiKey"];
        if (!string.IsNullOrEmpty(apiKey))
        {
            if (!Request.Headers.TryGetValue("X-Internal-Api-Key", out var provided) ||
                provided != apiKey)
                return Unauthorized();
        }

        if (file is null || file.Length == 0)
            return BadRequest("Dosya bulunamadı veya boş.");

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, ct);
        var fileId = await _cache.StoreDocumentAsync(ms.ToArray(), file.FileName, file.ContentType, ct);

        return Ok(new { fileId, downloadUrl = $"/api/docs/{fileId}" });
    }

    /// <summary>
    /// Cache'deki belgeyi indirir. Belgeler 30 dakika sonra otomatik silinir.
    /// </summary>
    [HttpGet("api/docs/{fileId}")]
    public async Task<IActionResult> DownloadAsync(string fileId, CancellationToken ct)
    {
        var doc = await _cache.GetDocumentAsync(fileId, ct);
        if (doc is null)
            return NotFound("Belge bulunamadı veya süresi dolmuş.");

        return File(doc.Value.Content, doc.Value.ContentType, doc.Value.FileName, enableRangeProcessing: true);
    }

    /// <summary>
    /// Büyük belgeleri HTTP bağlantısını bloklamadan kuyruğa alır.
    /// Sonuç SignalR üzerinden sessionId grubuna iletilir.
    /// </summary>
    [HttpPost("api/documents/upload-async")]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> UploadAsync(
        IFormFile file,
        [FromForm] string message,
        [FromServices] ChannelWriter<OcrJob> channelWriter,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest("Dosya boş veya eksik.");

        if (file.Length > 10 * 1024 * 1024)
            return BadRequest("Dosya boyutu 10 MB sınırını aşıyor.");

        var sessionId = HttpContext.Items["SessionId"]?.ToString() ?? Guid.NewGuid().ToString("N");
        var tempPath = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}");

        await using (var stream = System.IO.File.Create(tempPath))
            await file.CopyToAsync(stream, ct);

        var job = OcrJob.Create(sessionId, tempPath, file.FileName, message);
        await channelWriter.WriteAsync(job, ct);

        return Accepted(
            $"/api/documents/jobs/{job.JobId}",
            new { job.JobId, sessionId });
    }
}
