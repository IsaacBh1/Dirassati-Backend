using AutoMapper;
using Dirassati_Backend.Common.Dtos;
using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Features.School.DTOs;


namespace Dirassati_Backend.Features.School.MappingProfiles;

public class SchoolProfiles : Profile
{
    public SchoolProfiles()
    {

        CreateMap<PhoneNumber, PhoneNumberDTO>().ReverseMap();
        CreateMap<Address, AddressDto>().ReverseMap()
        .ForMember(dst => dst.AdresseId, opt => opt.Ignore());


        CreateMap<AcademicYear, AcademicYearDto>();
        CreateMap<AcademicYear, AcademicYearDto>().ReverseMap();

        CreateMap<UpdateSchoolInfosDTO, Data.Models.School>()
        .ForMember(s => s.PhoneNumbers, opt => opt.Ignore())
        .ForMember(s => s.Specializations, opt => opt.Ignore())
        .ForMember(s => s.Address, opt => opt.Ignore());

        CreateMap<Data.Models.School, GetSchoolInfoDTO>()
            .ForMember(dest => dest.AcademicYear,
                  opt => opt.MapFrom(src => src.AcademicYear));
        CreateMap<Data.Models.School, GetSchoolInfoDTO>();

    }


}
