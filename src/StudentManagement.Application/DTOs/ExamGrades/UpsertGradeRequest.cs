namespace StudentManagement.Application.DTOs;

public record UpsertGradeRequest(
    string StudentNameOrNumber,
    string CourseName,
    decimal? Exam1Grade,
    decimal? Exam2Grade);
