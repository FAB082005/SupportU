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
		private readonly IServiceAsignacion _serviceAsignacion; // ✅ AGREGADO
		private readonly IMapper _mapper;

		public ServiceTicket(
			IRepositoryTicket repository,
			IRepositoryCategoria repositoryCategoria,
			IServiceHistorialEstados serviceHistorial,
			IServiceNotificacion serviceNotificacion,
			IServiceAsignacion serviceAsignacion, // ✅ AGREGADO
			IMapper mapper)
		{
			_repository = repository;
			_repositoryCategoria = repositoryCategoria;
			_serviceHistorial = serviceHistorial;
			_serviceNotificacion = serviceNotificacion;
			_serviceAsignacion = serviceAsignacion; // ✅ AGREGADO
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

			// Crear historial de creación
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

			// ✅ CREAR ASIGNACIÓN AUTOMÁTICA (sin técnico asignado aún)
			try
			{
				var asignacionDTO = new AsignacionDTO
				{
					TicketId = ticketId,
					TecnicoId = 0, // Temporal, se actualizará cuando se asigne
					MetodoAsignacion = "Pendiente", // Indica que aún no ha sido asignado
					FechaAsignacion = DateTime.Now,
					UsuarioAsignadorId = dto.UsuarioSolicitanteId
				};

				await _serviceAsignacion.AddAsync(asignacionDTO);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error al crear asignación inicial: {ex.Message}");
			}

			// Crear notificación
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

		public async Task ActualizarCumplimientoSLAAsync(int ticketId)
		{
			var ticket = await _repository.FindByIdAsyncForUpdate(ticketId);
			if (ticket == null) return;

			var categoria = await _repositoryCategoria.FindByIdAsync(ticket.CategoriaId);
			if (categoria?.Sla == null) return;

			var tiempoRespuestaMinutos = categoria.Sla.TiempoRespuestaMinutos;
			var tiempoResolucionMinutos = categoria.Sla.TiempoResolucionMinutos;

			// 1. Calcular cumplimiento de respuesta
			if (ticket.fecha_primera_respuesta.HasValue)
			{
				var fechaLimiteRespuesta = ticket.FechaCreacion.AddMinutes(tiempoRespuestaMinutos);
				ticket.CumplimientoRespuesta = ticket.fecha_primera_respuesta <= fechaLimiteRespuesta;
			}

			// 2. Calcular cumplimiento de resolución
			if (ticket.fecha_resolucion.HasValue)
			{
				var fechaLimiteResolucion = ticket.FechaCreacion.AddMinutes(tiempoResolucionMinutos);
				ticket.CumplimientoResolucion = ticket.fecha_resolucion <= fechaLimiteResolucion;
			}
			else if (ticket.FechaCierre.HasValue)
			{
				// Si no hay fecha_resolucion pero sí fecha_cierre, usar esta
				var fechaLimiteResolucion = ticket.FechaCreacion.AddMinutes(tiempoResolucionMinutos);
				ticket.CumplimientoResolucion = ticket.FechaCierre <= fechaLimiteResolucion;
			}

			await _repository.UpdateAsync(ticket);
		}
	}
}