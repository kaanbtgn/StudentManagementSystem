namespace StudentManagement.Application.DTOs;

/// <summary>
/// pg_trgm similarity aramasından dönen tek sonuç:
/// eşleşen öğrenci bilgisi + benzerlik skoru (0–1).
/// </summary>
public sealed record FuzzyStudentMatch(StudentDto Student, double Score);
