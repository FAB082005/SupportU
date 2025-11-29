using AutoMapper;
using SupportU.Application.DTOs;
using SupportU.Infraestructure.Models;

namespace SupportU.Application.Profiles
{
    public class EspecialidadesProfiles : Profile
    {
        public EspecialidadesProfiles()
        {
            CreateMap<Especialidad, EspecialidadDTO>().ReverseMap();
        }
    }
}
