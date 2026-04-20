using System.ComponentModel;

namespace StudentManagement.MCP.Models;

public enum DocumentOutputFormat
{
    [Description("Microsoft Word belgesi (.docx)")]
    Word,

    [Description("Microsoft Excel elektronik tablosu (.xlsx)")]
    Excel,

    [Description("PDF belgesi (.pdf)")]
    Pdf,
}
