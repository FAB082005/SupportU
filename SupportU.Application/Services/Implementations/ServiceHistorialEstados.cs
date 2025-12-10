using AutoMapper;
using Microsoft.Extensions.Logging;
using SupportU.Application.DTOs;
using SupportU.Application.Services.Interfaces;
using SupportU.Infraestructure.Models;
using SupportU.Infraestructure.Repository.Interfaces;
using SupportU.Infrastructure.Repository;

namespace SupportU.Application.Services.Implementations
{
	public class ServiceHistorialEstados : IServiceHistorialEstados
	{
		private readonly IRepositoryHistorialEstados _repo;
		private readonly IRepositoryTicket _repoTicket;
		private readonly IMapper _mapper;
		private readonly IServiceImagen _serviceImagen;
		private readonly IRepositoryTecnico _repoTecnico;
		private readonly IServiceNotificacion _serviceNotificacion;
		private readonly ILogger<ServiceHistorialEstados> _logger;

		public ServiceHistorialEstados(
			IRepositoryHistorialEstados repo,
			IRepositoryTicket repoTicket,
			IServiceImagen serviceImagen,
			IRepositoryTecnico repoTecnico,
			IServiceNotificacion serviceNotificacion,
			IMapper mapper,
			ILogger<ServiceHistorialEstados> logger)
		{
			_repo = repo;
			_repoTicket = repoTicket;
			_serviceImagen = serviceImagen;
			_mapper = mapper;
			_repoTecnico = repoTecnico;
			_serviceNotificacion = serviceNotificacion;
			_logger = logger;
		}

		public async Task<ICollection<HistorialEstadosDTO>> ListAsync()
		{
			var list = await _repo.ListAsync();
			var dtos = _mapper.Map<ICollection<HistorialEstadosDTO>>(list);

			foreach (var dto in dtos)
			{
				dto.Imagenes = await _serviceImagen.GetByHistorialIdAsync(dto.HistorialId);
			}
			return dtos;
		}

		public async Task<HistorialEstadosDTO?> FindByIdAsync(int id)
		{
			var entity = await _repo.FindByIdAsync(id);
			if (entity == null) return null;

			var dto = _mapper.Map<HistorialEstadosDTO>(entity);
			dto.Imagenes = await _serviceImagen.GetByHistorialIdAsync(id);
			return dto;
		}

		public async Task<int> AddAsync(HistorialEstadosDTO dto)
		{
			try
			{
				_logger.LogInformation("Iniciando cambio de estado para ticket {TicketId} de {EstadoAnterior} a {EstadoNuevo}",
					dto.TicketId, dto.EstadoAnterior, dto.EstadoNuevo);

				var ticket = await _repoTicket.FindByIdAsyncForUpdate(dto.TicketId);
				if (ticket == null)
				{
					throw new KeyNotFoundException($"Ticket {dto.TicketId} no encontrado");
				}

				// Actualiza el estado del ticket
				ticket.Estado = dto.EstadoNuevo;

				await CalcularCumplimientoSLA(ticket, dto.EstadoNuevo);

				// Guarda los cambios del ticket
				await _repoTicket.UpdateAsync(ticket);
				_logger.LogInformation("Estado del ticket actualizado, SLA calculado exitosamente");

				var entity = new HistorialEstado
				{
					TicketId = dto.TicketId,
					EstadoAnterior = dto.EstadoAnterior,
					EstadoNuevo = dto.EstadoNuevo,
					UsuarioId = dto.UsuarioId,
					Observaciones = dto.Observaciones,
					FechaCambio = dto.FechaCambio
				};

				var historialId = await _repo.AddAsync(entity);
				_logger.LogInformation("Historial de cambio de estado creado con ID {HistorialId}", historialId);

				// Guarda las imágenes
				if (dto.Imagenes != null && dto.Imagenes.Any())
				{
					foreach (var imagenDto in dto.Imagenes)
					{
						imagenDto.TicketId = dto.TicketId;
						imagenDto.HistorialEstadoId = historialId;

						var imagenId = await _serviceImagen.AddAsync(imagenDto);
					}

					_logger.LogInformation("Se guardaron {Count} imagenes asociadas al cambio de estado", dto.Imagenes.Count);
				}
				else
				{
					_logger.LogWarning("No se recibieron imagenes para guardar en el historial");
				}

				await GenerarNotificacionesCambioEstadoAsync(dto);

				return historialId;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al agregar historial de estado para ticket {TicketId}", dto.TicketId);
				_logger.LogError("Detalle del error: {Message}", ex.Message);
				if (ex.InnerException != null)
				{
					_logger.LogError("Inner exception: {InnerMessage}", ex.InnerException.Message);
				}
				throw;
			}
		}

		private async Task CalcularCumplimientoSLA(Ticket ticket, string nuevoEstado)
		{
			try
			{
				var ahora = DateTime.Now;
				var ticketCompleto = await _repoTicket.FindByIdAsync(ticket.TicketId);
				if (ticketCompleto?.Categoria?.Sla == null)
				{
					_logger.LogWarning("No se encontro configuracion de SLA para el ticket {TicketId}", ticket.TicketId);
					return;
				}

				// Obtiene los tiempos de SLA
				var tiempoRespuestaMinutos = ticketCompleto.Categoria.Sla.TiempoRespuestaMinutos;
				var tiempoResolucionMinutos = ticketCompleto.Categoria.Sla.TiempoResolucionMinutos;

				// Calcula las fechas límite
				var fechaLimiteRespuesta = ticket.FechaCreacion.AddMinutes(tiempoRespuestaMinutos);
				var fechaLimiteResolucion = ticket.FechaCreacion.AddMinutes(tiempoResolucionMinutos);

				_logger.LogInformation("Verificando SLA del ticket {TicketId}. Creado: {FechaCreacion}, Limite respuesta: {LimiteRespuesta}, Limite resolucion: {LimiteResolucion}",
					ticket.TicketId, ticket.FechaCreacion, fechaLimiteRespuesta, fechaLimiteResolucion);

				if (nuevoEstado != "Pendiente" && !ticket.fecha_primera_respuesta.HasValue)
				{
					ticket.fecha_primera_respuesta = ahora;
					ticket.CumplimientoRespuesta = ahora <= fechaLimiteRespuesta;

					var estadoCumplimiento = ticket.CumplimientoRespuesta.Value ? "dentro del tiempo" : "fuera del tiempo";
					_logger.LogInformation("Primera respuesta registrada para ticket {TicketId} el {Fecha}, {Estado} establecido",
						ticket.TicketId, ahora, estadoCumplimiento);
				}
				if (nuevoEstado == "Resuelto" && !ticket.fecha_resolucion.HasValue)
				{
					ticket.fecha_resolucion = ahora;
					ticket.CumplimientoResolucion = ahora <= fechaLimiteResolucion;

					var estadoCumplimiento = ticket.CumplimientoResolucion.Value ? "cumpliendo" : "excediendo";
					_logger.LogInformation("Ticket {TicketId} resuelto el {Fecha}, {Estado} el tiempo de resolucion del SLA",
						ticket.TicketId, ahora, estadoCumplimiento);
				}
				if (nuevoEstado == "Cerrado")
				{
					// Marcar fecha de cierre
					if (!ticket.FechaCierre.HasValue)
					{
						ticket.FechaCierre = ahora;
						_logger.LogInformation("Ticket {TicketId} cerrado el {Fecha}", ticket.TicketId, ahora);
					}

					if (!ticket.fecha_resolucion.HasValue)
					{
						ticket.fecha_resolucion = ahora;
						ticket.CumplimientoResolucion = ahora <= fechaLimiteResolucion;

						var diferencia = (ahora - fechaLimiteResolucion).TotalMinutes;
						var estadoCumplimiento = ticket.CumplimientoResolucion.Value ? "cumpliendo" : "excediendo";
						_logger.LogInformation("Resolucion automatica al cerrar ticket {TicketId}. Diferencia con limite: {Diferencia} minutos, {Estado} SLA",
							ticket.TicketId, diferencia, estadoCumplimiento);
					}
					else
					{
						_logger.LogInformation("Ticket {TicketId} ya tenia fecha de resolucion previa: {FechaResolucion}",
							ticket.TicketId, ticket.fecha_resolucion.Value);
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al calcular cumplimiento de SLA para ticket {TicketId}", ticket.TicketId);
			}
		}

		private async Task GenerarNotificacionesCambioEstadoAsync(HistorialEstadosDTO dto)
		{
			try
			{
				var ticket = await _repoTicket.FindByIdAsync(dto.TicketId);
				if (ticket == null)
				{
					_logger.LogWarning("No se pudo obtener informacion del ticket {TicketId} para generar notificaciones", dto.TicketId);
					return;
				}

				try
				{
					string mensajeCliente = ObtenerMensajePorEstado(dto.EstadoAnterior, dto.EstadoNuevo, ticket.Titulo);

					await _serviceNotificacion.CreateNotificationAsync(
						usuarioDestinatarioId: ticket.UsuarioSolicitanteId,
						ticketId: dto.TicketId,
						tipo: "CambioEstado",
						mensaje: mensajeCliente
					);

					_logger.LogInformation("Notificacion de cambio de estado enviada al usuario solicitante {UsuarioId}", ticket.UsuarioSolicitanteId);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error al crear notificacion para el usuario solicitante");
				}

				if (ticket.TecnicoAsignadoId.HasValue)
				{
					try
					{
						var tecnico = await _repoTecnico.FindByIdAsync(ticket.TecnicoAsignadoId.Value);

						if (tecnico != null)
						{
							string mensajeTecnico = ObtenerMensajeParaTecnico(dto.EstadoAnterior, dto.EstadoNuevo, ticket.Titulo, dto.TicketId);

							await _serviceNotificacion.CreateNotificationAsync(
								usuarioDestinatarioId: tecnico.UsuarioId,
								ticketId: dto.TicketId,
								tipo: "CambioEstado",
								mensaje: mensajeTecnico
							);

							_logger.LogInformation("Notificacion de cambio de estado enviada al tecnico asignado {UsuarioId}", tecnico.UsuarioId);
						}
						else
						{
							_logger.LogWarning("No se encontro informacion del tecnico con ID {TecnicoId}", ticket.TecnicoAsignadoId.Value);
						}
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "Error al crear notificacion para el tecnico asignado");
					}
				}
				else
				{
					_logger.LogInformation("El ticket {TicketId} no tiene tecnico asignado, omitiendo notificacion", dto.TicketId);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error general al generar notificaciones de cambio de estado para ticket {TicketId}", dto.TicketId);
			}
		}

		private string ObtenerMensajePorEstado(string? estadoAnterior, string estadoNuevo, string tituloTicket)
		{
			return estadoNuevo switch
			{
				"Pendiente" => $"Tu ticket '{tituloTicket}' ha sido creado y está pendiente de asignación",
				"Asignado" => $"Tu ticket '{tituloTicket}' ha sido asignado a un técnico",
				"En Proceso" => $"Tu ticket '{tituloTicket}' está siendo atendido por un técnico",
				"Resuelto" => $"Tu ticket '{tituloTicket}' ha sido resuelto. Por favor verifica la solución",
				"Cerrado" => $"Tu ticket '{tituloTicket}' ha sido cerrado",
				_ => $"Tu ticket '{tituloTicket}' cambió de estado: {estadoAnterior ?? "Nuevo"} → {estadoNuevo}"
			};
		}

		private string ObtenerMensajeParaTecnico(string? estadoAnterior, string estadoNuevo, string tituloTicket, int ticketId)
		{
			return estadoNuevo switch
			{
				"Asignado" => $"Se te ha asignado el ticket #{ticketId}: '{tituloTicket}'",
				"En Proceso" => $"El ticket #{ticketId}: '{tituloTicket}' que tienes asignado cambió a En Proceso",
				"Resuelto" => $"El ticket #{ticketId}: '{tituloTicket}' ha sido marcado como Resuelto",
				"Cerrado" => $"El ticket #{ticketId}: '{tituloTicket}' ha sido cerrado",
				_ => $"El ticket #{ticketId}: '{tituloTicket}' que tienes asignado cambió de estado: {estadoAnterior ?? "Nuevo"} → {estadoNuevo}"
			};
		}

		public async Task UpdateAsync(HistorialEstadosDTO dto)
		{
			var entity = await _repo.FindByIdAsync(dto.HistorialId);
			if (entity == null) throw new KeyNotFoundException("Historial de estado no encontrado");

			entity.EstadoAnterior = dto.EstadoAnterior;
			entity.EstadoNuevo = dto.EstadoNuevo;
			entity.UsuarioId = dto.UsuarioId;
			entity.Observaciones = dto.Observaciones;
			entity.FechaCambio = dto.FechaCambio;
			entity.TicketId = dto.TicketId;

			await _repo.UpdateAsync(entity);
		}

		public async Task DeleteAsync(int id)
		{
			// Implementar si es necesario
		}
	}
}