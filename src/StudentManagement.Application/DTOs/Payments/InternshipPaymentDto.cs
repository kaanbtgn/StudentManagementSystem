namespace StudentManagement.Application.DTOs;

public record InternshipPaymentDto(
    Guid Id,
    Guid StudentId,
    string StudentName,
    int PeriodYear,
    int PeriodMonth,
    decimal Amount,
    DateOnly? PaymentDate,
    string Status);
