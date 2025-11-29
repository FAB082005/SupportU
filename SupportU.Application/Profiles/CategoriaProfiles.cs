using AutoMapper;
using SupportU.Application.DTOs;
using SupportU.Infraestructure.Models;

namespace SupportU.Application.Profiles
{
    public class CategoriasProfiles : Profile
    {
        public CategoriasProfiles()
        {
            CreateMap<Categoria, CategoriaDTO>()
               .ForMember(d => d.SlaNombre, opt => opt.MapFrom(s => s.Sla != null ? s.Sla.Nombre : null))
               .ForMember(d => d.CriterioAsignacion, opt => opt.MapFrom(s => s.CriterioAsignacion))
               .ForMember(d => d.Activa, opt => opt.MapFrom(s => s.Activa));
        }
    }
}
