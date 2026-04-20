namespace StudentManagement.Application.DTOs;

public record StudentDto(
    Guid Id,
    string FirstName,
    string LastName,
    string StudentNumber,
    string Department,
    string? Phone,
    DateOnly EnrollmentDate);
