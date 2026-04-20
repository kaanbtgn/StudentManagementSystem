namespace StudentManagement.Application.DTOs;

public record UpdateStudentRequest(
    string? FirstName,
    string? LastName,
    string? Department,
    string? Phone);
