using AutoMapper;
using Dirassati_Backend.Common.Dtos;
using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Features.Teachers.Dtos;

namespace Dirassati_Backend.Mappings;

public class TeacherProfile : Profile
{
    public TeacherProfile()
    {
        CreateMap<Teacher, GetTeacherInfosDto>()
        .ForMember(dst => dst.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
        .ForMember(dst => dst.LastName, opt => opt.MapFrom(src => src.User.LastName))
        .ForMember(dst => dst.ContractType, opt => opt.MapFrom(src => src.ContractType.Name))
        .ForMember(dst => dst.Email, opt => opt.MapFrom(src => src.User.Email))
        .ForMember(dst => dst.TeacherId, opt => opt.MapFrom(src => src.TeacherId.ToString()));
        CreateMap<Student, BasePersonDto>();
        CreateMap<Teacher, BasePersonDto>();
        CreateMap<StudentReport, GetStudentReportDto>()
        .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.StudentReportId))
        .ForMember(dst => dst.Student, opt => opt.MapFrom(src => src.Student))

        ;
        CreateMap<StudentReport, StudentReport>();
    }
}
