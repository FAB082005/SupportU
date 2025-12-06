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
	public class NotificacionProfile : Profile
	{
		public NotificacionProfile() {
			CreateMap<Notificacion, NotificacionDTO>()
					 .ForMember(dest => dest.NombreDestinatario,
						 opt => opt.MapFrom(src => src.UsuarioDestinatario.Nombre + " " + src.UsuarioDestinatario.Apellidos))
					 .ForMember(dest => dest.TituloTicket,
						 opt => opt.MapFrom(src => src.Ticket != null ? src.Ticket.Titulo : string.Empty))
					 .ReverseMap();
		}
	}
}
