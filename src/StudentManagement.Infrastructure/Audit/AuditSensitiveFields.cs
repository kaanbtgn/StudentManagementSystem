namespace StudentManagement.Infrastructure.Audit;

/// <summary>
/// Audit snapshot alınırken maskelenmesi gereken hassas alan adlarını tanımlar (KVKK uyumu).
/// Alan adı karşılaştırması büyük/küçük harf duyarsızdır.
/// </summary>
public static class AuditSensitiveFields
{
    public static readonly HashSet<string> Masked = new(StringComparer.OrdinalIgnoreCase)
    {
        "Email",
        "Phone"
    };

    public const string MaskedValue = "[MASKED]";
}
