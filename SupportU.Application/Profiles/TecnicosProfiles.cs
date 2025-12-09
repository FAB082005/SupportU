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
                .ForMember(d => d.UsuarioId, opt => opt.MapFrom(s => s.UsuarioId))
                .ForMember(d => d.NombreUsuario, opt => opt.MapFrom(s => s.Usuario != null ? s.Usuario.Nombre : null))
                .ForMember(d => d.CorreoUsuario, opt => opt.MapFrom(s => s.Usuario != null ? s.Usuario.Email : null))
                .ForMember(d => d.Especialidades, opt => opt.MapFrom(s => s.Especialidad));

            CreateMap<TecnicoDTO, Tecnico>()
                .ForMember(d => d.Especialidad, opt => opt.Ignore())
                .ForMember(d => d.Usuario, opt => opt.Ignore());

            CreateMap<Especialidad, EspecialidadDTO>().ReverseMap();
        }
    }
}