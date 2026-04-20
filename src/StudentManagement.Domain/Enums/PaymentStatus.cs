using System.ComponentModel;

namespace StudentManagement.Domain.Enums;

/// <summary>
/// Staj burs ödemesinin mevcut durumunu tanımlar.
/// </summary>
public enum PaymentStatus : int
{
    /// <summary>Ödeme henüz gerçekleştirilmemiş, beklemede.</summary>
    [Description("Beklemede")]
    Pending = 0,

    /// <summary>Ödeme tamamlandı.</summary>
    [Description("Ödendi")]
    Paid = 1,

    /// <summary>Ödeme vadesi geçmiş.</summary>
    [Description("Gecikmiş")]
    Overdue = 2,

    /// <summary>Ödeme iptal edildi.</summary>
    [Description("İptal Edildi")]
    Cancelled = 3,
}
