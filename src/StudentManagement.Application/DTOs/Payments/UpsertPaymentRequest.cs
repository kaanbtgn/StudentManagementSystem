namespace StudentManagement.Application.DTOs;

public record UpsertPaymentRequest(
    string StudentNameOrNumber,
    int PeriodYear,
    int PeriodMonth,
    decimal Amount,
    DateOnly? PaymentDate);
