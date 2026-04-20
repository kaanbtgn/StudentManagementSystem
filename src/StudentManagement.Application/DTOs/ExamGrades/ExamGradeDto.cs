namespace StudentManagement.Application.DTOs;

public record ExamGradeDto(
    Guid Id,
    Guid StudentId,
    string StudentName,
    string CourseName,
    decimal? Exam1Grade,
    decimal? Exam2Grade);
