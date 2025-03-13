using System;
using Dirassati_Backend.Data.Models;

namespace Dirassati_Backend.Features.School.DTOs;

public class GetSchoolInfoDTO
{
    public Guid SchoolId { get; set; } = Guid.NewGuid();

    public required string Name { get; set; } = null!;

    public Address Address { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Logo { get; set; } = null!;

    public string WebsiteUrl { get; set; } = null!;

    public string SchoolConfig { get; set; } = null!;
    public virtual List<PhoneNumber> PhoneNumbers { get; set; } = [];

    public SchoolType SchoolType { get; set; } = null!;


    public AcademicYear? AcademicYear { get; set; } = null!;
}
