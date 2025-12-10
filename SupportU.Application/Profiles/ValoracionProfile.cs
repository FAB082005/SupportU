using AutoMapper;
using SupportU.Application.DTOs;
using SupportU.Infraestructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportU.Application.Profiles
{
	public class ValoracionProfile : Profile
	{
		public ValoracionProfile()
		{
			CreateMap<Valoracion, ValoracionDTO>()
				.ForMember(dest => dest.TicketTitulo,
					opt => opt.MapFrom(src => src.Ticket != null ? src.Ticket.Titulo : null))
				.ForMember(dest => dest.UsuarioNombre,
					opt => opt.MapFrom(src => src.Usuario != null ? src.Usuario.Nombre : null));

			CreateMap<ValoracionDTO, Valoracion>();
		}
	}
}
