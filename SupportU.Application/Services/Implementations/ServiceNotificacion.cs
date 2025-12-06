using AutoMapper;
using SupportU.Application.DTOs;
using SupportU.Application.Services.Interfaces;
using SupportU.Infraestructure.Models;
using SupportU.Infraestructure.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SupportU.Application.Services.Implementations
{
	public class ServiceNotificacion : IServiceNotificacion
	{
		private readonly IRepositoryNotificacion _repo;
		private readonly IMapper _mapper;

		public ServiceNotificacion(IRepositoryNotificacion repo, IMapper mapper)
		{
			_repo = repo;
			_mapper = mapper;
		}

		public async Task<List<NotificacionDTO>> GetByUserIdAsync(int usuarioId)
		{
			var list = await _repo.ListByUserIdAsync(usuarioId);
			return _mapper.Map<List<NotificacionDTO>>(list);
		}

		public async Task<int> GetPendingCountAsync(int usuarioId)
		{
			return await _repo.CountPendingByUserIdAsync(usuarioId);
		}

		public async Task<bool> MarkAsReadAsync(int notificacionId, int usuarioId)
		{
			return await _repo.MarkAsReadAsync(notificacionId, usuarioId);
		}

		public async Task CreateNotificationAsync(int usuarioDestinatarioId, int? ticketId, string tipo, string mensaje)
		{
			var notificacion = new Notificacion
			{
				UsuarioDestinatarioId = usuarioDestinatarioId,
				TicketId = ticketId,
				TipoNotificacion = tipo,
				Mensaje = mensaje,
				Estado = "Pendiente",
				FechaCreacion = DateTime.Now
			};

			await _repo.CreateAsync(notificacion);
		}
	}
}