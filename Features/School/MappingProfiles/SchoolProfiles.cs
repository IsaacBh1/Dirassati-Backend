using System;
using AutoMapper;
using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Features.School.DTOs;


namespace Dirassati_Backend.Features.School.MappingProfiles;

public class SchoolProfiles : Profile
{
    public SchoolProfiles()
    {
        CreateMap<Data.Models.School, GetSchoolInfoDTO>()
            .ForMember(dest => dest.AcademicYear,
                  opt => opt.MapFrom(src => src.AcademicYear));
    }


}
