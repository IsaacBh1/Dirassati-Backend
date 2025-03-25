using System;
using AutoMapper;

namespace Dirassati_Backend.Mappings;

public class TeacherProfile : Profile
{
    public TeacherProfile()
    {
        CreateMap<Teacher, GetTeacherInfosDTO>()
        .ForMember(dst => dst.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
        .ForMember(dst => dst.LastName, opt => opt.MapFrom(src => src.User.LastName))
        .ForMember(dst => dst.ContractType, opt => opt.MapFrom(src => src.ContractType.Name))
        .ForMember(dst => dst.Email, opt => opt.MapFrom(src => src.User.Email))
        .ForMember(dst => dst.TeacherId, opt => opt.MapFrom(src => src.TeacherId.ToString()))

       ;
    }
}
