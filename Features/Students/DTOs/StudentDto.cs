namespace Dirassati_Backend.Features.Students.DTOs;

public record StudentDto(
    Guid StudentId,
    string FirstName,
    string LastName,
    string StudentIdNumber,
    string ParentFullName,
    byte[]? PhotoUrl,
    string ParentContact
);