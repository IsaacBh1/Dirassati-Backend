using System;
using AutoMapper;
using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Features.School.DTOs;


namespace Dirassati_Backend.Features.School.MappingProfiles;

public class SchoolProfiles : Profile
{
    public SchoolProfiles()
    {
        CreateMap<AcademicYear, AcademicYearDTO>();
        CreateMap<PhoneNumberDTO, PhoneNumber>();
        CreateMap<UpdateSchoolInfosDTO, Data.Models.School>()
        .ForMember(s => s.PhoneNumbers, opt => opt.MapFrom(u => u.PhoneNumbers))
        .ForMember(s => s.Specializations, opt => opt.Ignore());
        CreateMap<Data.Models.School, GetSchoolInfoDTO>()
            .ForMember(dest => dest.AcademicYear,
                  opt => opt.MapFrom(src => src.AcademicYear));
    }


}
