using System;
using Dirassati_Backend.Data.Models;


namespace Dirassati_Backend.Features.School.DTOs;

public class UpdateSchoolInfosDTO
{

    public required string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Logo { get; set; } = null!;

    public string WebsiteUrl { get; set; } = null!;

    public AcademicYear CurrentAcademicYear { get; set; } = null!;

    public Address Address { get; set; } = null!;
    public virtual ICollection<int> Specializations { get; set; } = null!;

}

