using System;

namespace Dirassati_Backend.Features.Students.DTOs
{
    public record StudentDetailsDto
    (
        Guid StudentId,
        string FirstName,
        string LastName,
        string Address,
        DateOnly BirthDate,
        string BirthPlace,
        Guid SchoolId,
        string? StudentIdNumber,
        string EmergencyContact,
        int SchoolLevelId,
        int? SpecializationId,
        int ParentRelationshipToStudentTypeId,
        byte[]? PhotoUrl,
        DateOnly EnrollmentDate,
        Guid ParentId,
        bool IsActive,
        int? GroupId
    );
}
