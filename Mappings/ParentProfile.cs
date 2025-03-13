using AutoMapper;
using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Features.Parents.Dtos;

namespace Dirassati_Backend.Mappings
{
    public class ParentProfile : Profile
    {
        public ParentProfile()
        {
            CreateMap<Parent, GetParentDto>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.User.BirthDate))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.NormalizedUserName, opt => opt.MapFrom(src => src.User.NormalizedUserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.NormalizedEmail, opt => opt.MapFrom(src => src.User.NormalizedEmail))
                .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => src.User.EmailConfirmed))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
                .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.MapFrom(src => src.User.PhoneNumberConfirmed));

            CreateMap<UpdateParentDto, Parent>()
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .AfterMap((src, dest) =>
            {
                if (dest.User != null)
                {
                    dest.User.FirstName = src.FirstName;
                    dest.User.LastName = src.LastName;
                    dest.User.Email = src.Email;
                    dest.User.PhoneNumber = src.PhoneNumber;
                }
            });

        }


    }
}