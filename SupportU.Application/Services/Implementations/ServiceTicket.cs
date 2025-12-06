using AutoMapper;
using SupportU.Application.DTOs;
using SupportU.Application.Services.Interfaces;
using SupportU.Infraestructure.Models;
using SupportU.Infraestructure.Repository.Interfaces;
using SupportU.Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SupportU.Application.Services.Implementations
{
	public class ServiceTicket : IServiceTicket
	{
		private readonly IRepositoryTicket _repository;
		private readonly IRepositoryCategoria _repositoryCategoria;
		private readonly IServiceHistorialEstados _serviceHistorial;
		private readonly IServiceNotificacion _serviceNotificacion; 
		private readonly IMapper _mapper;

		public ServiceTicket(
			IRepositoryTicket repository,
			IRepositoryCategoria repositoryCategoria,
			IServiceHistorialEstados serviceHistorial,
			IServiceNotificacion serviceNotificacion, 
			IMapper mapper)
		{
			_repository = repository;
			_repositoryCategoria = repositoryCategoria;
			_serviceHistorial = serviceHistorial;
			_serviceNotificacion = serviceNotificacion; 
			_mapper = mapper;
		}

		public async Task<ICollection<TicketDTO>> ListAsync()
		{
			var list = await _repository.ListAsync();
			var collection = _mapper.Map<List<TicketDTO>>(list);
			return collection;
		}

		public async Task<TicketDTO?> FindByIdAsync(int id)
		{
			var entity = await _repository.FindByIdAsync(id);
			if (entity == null) return null;
			var dto = _mapper.Map<TicketDTO>(entity);
			return dto;
		}

		public async Task<int> AddAsync(TicketDTO dto)
		{
			var categoria = await _repositoryCategoria.FindByIdAsync(dto.CategoriaId);
			if (categoria == null)
				throw new KeyNotFoundException("Categoría no encontrada");

			// Construir la entidad del ticket
			var entity = new Ticket
			{
				Titulo = dto.Titulo?.Trim() ?? string.Empty,
				Descripcion = dto.Descripcion?.Trim() ?? string.Empty,
				UsuarioSolicitanteId = dto.UsuarioSolicitanteId,
				CategoriaId = dto.CategoriaId,
				TecnicoAsignadoId = null,
				Prioridad = dto.Prioridad ?? "Media",
				Estado = "Pendiente",
				FechaCreacion = DateTime.Now,
				FechaCierre = null,
				CumplimientoRespuesta = null,
				CumplimientoResolucion = null,
				fecha_primera_respuesta = null,
				fecha_resolucion = null
			};

			// Guardar el ticket
			var ticketId = await _repository.AddAsync(entity);


			var historialCreacion = new HistorialEstadosDTO
			{
				TicketId = ticketId,
				EstadoAnterior = null,
				EstadoNuevo = "Pendiente",
				UsuarioId = dto.UsuarioSolicitanteId,
				Observaciones = "Ticket creado por el usuario",
				FechaCambio = DateTime.Now
	
			};

			await _serviceHistorial.AddAsync(historialCreacion);

			try
			{
				await _serviceNotificacion.CreateNotificationAsync(
					usuarioDestinatarioId: dto.UsuarioSolicitanteId,
					ticketId: ticketId,
					tipo: "TicketCreado",
					mensaje: $"Tu ticket #{ticketId} - '{entity.Titulo}' ha sido creado exitosamente"
				);
			}
			catch (Exception ex)
			{

				Console.WriteLine($"Error al crear notificación: {ex.Message}");
			}

			return ticketId;
		}

		public async Task UpdateAsync(TicketDTO dto)
		{
			var entity = await _repository.FindByIdAsyncForUpdate(dto.TicketId);
			if (entity == null)
				throw new KeyNotFoundException("Ticket no encontrado");

			entity.Titulo = dto.Titulo?.Trim() ?? entity.Titulo;
			entity.Descripcion = dto.Descripcion?.Trim() ?? entity.Descripcion;
			entity.CategoriaId = dto.CategoriaId;
			entity.TecnicoAsignadoId = dto.TecnicoAsignadoId;
			entity.Prioridad = dto.Prioridad ?? entity.Prioridad;
			entity.Estado = dto.Estado ?? entity.Estado;
			entity.FechaCierre = dto.FechaCierre;
			entity.CumplimientoRespuesta = dto.CumplimientoRespuesta;
			entity.CumplimientoResolucion = dto.CumplimientoResolucion;
			entity.fecha_primera_respuesta = dto.fecha_primera_respuesta;
			entity.fecha_resolucion = dto.fecha_resolucion;

			await _repository.UpdateAsync(entity);

			try
			{
				await _serviceNotificacion.CreateNotificationAsync(
					usuarioDestinatarioId: dto.UsuarioSolicitanteId,
					ticketId: dto.TicketId,
					tipo: "TicketCreado",
					mensaje: $"Tu ticket #{dto.TicketId} - '{entity.Titulo}' ha sido Actualizado correctamente"
				);
			}
			catch (Exception ex)
			{

				Console.WriteLine($"Error al crear notificación: {ex.Message}");
			}
		}

		public async Task DeleteAsync(int id)
		{
			// Implementar si es necesario
		}
	}
}