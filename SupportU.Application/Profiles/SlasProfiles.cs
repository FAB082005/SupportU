using AutoMapper;
using SupportU.Application.DTOs;
using SupportU.Infraestructure.Models;

namespace SupportU.Application.Profiles
{
    public class SlasProfiles : Profile
    {
        public SlasProfiles()
        {
            CreateMap<Sla, SlaDTO>().ReverseMap();
        }
    }
}

