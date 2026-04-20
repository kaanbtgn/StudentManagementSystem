using StudentManagement.MCP.Models;

namespace StudentManagement.MCP.Services;

public interface IWordGenerator
{
    (byte[] Content, string FileName, string ContentType) Generate(StandardDocumentContent content);
}

public interface IExcelGenerator
{
    (byte[] Content, string FileName, string ContentType) Generate(ExcelDocumentContent content);
}

public interface IPdfGenerator
{
    (byte[] Content, string FileName, string ContentType) Generate(StandardDocumentContent content);
}
