using AutoMapper;
using SupportU.Application.DTOs;
using SupportU.Infraestructure.Models;

namespace SupportU.Application.Profiles
{
    public class TicketProfile : Profile
    {
        public TicketProfile()
        {
			CreateMap<Ticket, TicketDTO>()
			  
			  .ForMember(dest => dest.CategoriaNombre,
						opt => opt.MapFrom(src => src.Categoria != null ? src.Categoria.Nombre : null));
		}
    }
}