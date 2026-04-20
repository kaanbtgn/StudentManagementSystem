namespace StudentManagement.Application.DTOs;

public record CreateStudentRequest(
    string FirstName,
    string LastName,
    string StudentNumber,
    string Department,
    string? Phone,
    DateOnly EnrollmentDate);
