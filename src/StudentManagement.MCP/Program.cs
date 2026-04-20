using QuestPDF.Infrastructure;
using Scalar.AspNetCore;
using StudentManagement.MCP;
using StudentManagement.MCP.Services;

// QuestPDF Community lisansı — ücretsiz kullanım için zorunlu
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddMcpHttpClients(builder.Configuration);

// Belge jeneratörleri
builder.Services.AddSingleton<IWordGenerator, WordGenerator>();
builder.Services.AddSingleton<IExcelGenerator, ExcelGenerator>();
builder.Services.AddSingleton<IPdfGenerator, PdfGenerator>();

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/", () => Results.Redirect("/scalar/v1"));

// SSE + Streamable HTTP transport — Agent ve Copilot bu endpoint üzerinden bağlanır
app.MapMcp("/mcp");

await app.RunAsync();
