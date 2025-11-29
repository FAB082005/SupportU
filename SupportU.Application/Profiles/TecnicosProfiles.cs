using AutoMapper;
using SupportU.Application.DTOs;
using SupportU.Infraestructure.Models;

namespace SupportU.Application.Profiles
{
    public class TecnicosProfiles : Profile
    {
        public TecnicosProfiles()
        {
            CreateMap<Tecnico, TecnicoDTO>()
                .ForMember(d => d.NombreUsuario, opt => opt.MapFrom(s => s.Usuario != null ? s.Usuario.Nombre : null))
                .ForMember(d => d.EmailUsuario, opt => opt.MapFrom(s => s.Usuario != null ? s.Usuario.Email : null))
                .ReverseMap();
        }
    }
}
