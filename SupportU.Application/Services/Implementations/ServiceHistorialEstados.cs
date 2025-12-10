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
				_logger.LogInformation("🔄 Iniciando cambio de estado para Ticket {TicketId}: {EstadoAnterior} → {EstadoNuevo}",
					dto.TicketId, dto.EstadoAnterior, dto.EstadoNuevo);

				// 🔴 IMPORTANTE: Obtener y actualizar el ticket con cálculo de SLA
				var ticket = await _repoTicket.FindByIdAsyncForUpdate(dto.TicketId);
				if (ticket == null)
				{
					throw new KeyNotFoundException($"Ticket {dto.TicketId} no encontrado");
				}

				// Actualizar el estado del ticket
				ticket.Estado = dto.EstadoNuevo;

				// 🔴 CALCULAR CUMPLIMIENTO DE SLA
				await CalcularCumplimientoSLA(ticket, dto.EstadoNuevo);

				// Guardar los cambios del ticket
				await _repoTicket.UpdateAsync(ticket);
				_logger.LogInformation("✅ Ticket actualizado con nuevo estado y SLA calculado");

				// Crear el historial
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
				_logger.LogInformation("✅ Historial creado con ID: {HistorialId}", historialId);

				// Guardar imágenes
				if (dto.Imagenes != null && dto.Imagenes.Any())
				{
					foreach (var imagenDto in dto.Imagenes)
					{
						imagenDto.TicketId = dto.TicketId;
						imagenDto.HistorialEstadoId = historialId;

						var imagenId = await _serviceImagen.AddAsync(imagenDto);
					}

					_logger.LogInformation("✅ {Count} imágenes guardadas exitosamente", dto.Imagenes.Count);
				}
				else
				{
					_logger.LogWarning("⚠️ No se recibieron imágenes para guardar");
				}

				// Generar notificaciones
				await GenerarNotificacionesCambioEstadoAsync(dto);

				return historialId;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "❌ ERROR en ServiceHistorialEstados.AddAsync");
				_logger.LogError("Mensaje: {Message}", ex.Message);
				_logger.LogError("InnerException: {InnerException}", ex.InnerException?.Message);
				throw;
			}
		}

		/// <summary>
		/// 🔴 NUEVO MÉTODO: Calcula y actualiza el cumplimiento de SLA según el nuevo estado
		/// </summary>
		private async Task CalcularCumplimientoSLA(Ticket ticket, string nuevoEstado)
		{
			try
			{
				var ahora = DateTime.Now;

				// Obtener el ticket completo con su categoría y SLA (FindByIdAsync devuelve Ticket, no DTO)
				var ticketCompleto = await _repoTicket.FindByIdAsync(ticket.TicketId);
				if (ticketCompleto?.Categoria?.Sla == null)
				{
					_logger.LogWarning("⚠️ No se encontró SLA para el ticket {TicketId}", ticket.TicketId);
					return;
				}

				// Obtener tiempos de SLA
				var tiempoRespuestaMinutos = ticketCompleto.Categoria.Sla.TiempoRespuestaMinutos;
				var tiempoResolucionMinutos = ticketCompleto.Categoria.Sla.TiempoResolucionMinutos;

				// Calcular fechas límite
				var fechaLimiteRespuesta = ticket.FechaCreacion.AddMinutes(tiempoRespuestaMinutos);
				var fechaLimiteResolucion = ticket.FechaCreacion.AddMinutes(tiempoResolucionMinutos);

				_logger.LogInformation("📊 SLA del Ticket {TicketId}:", ticket.TicketId);
				_logger.LogInformation("   • Creado: {FechaCreacion}", ticket.FechaCreacion);
				_logger.LogInformation("   • Límite Respuesta: {FechaLimite} ({Minutos} min)",
					fechaLimiteRespuesta, tiempoRespuestaMinutos);
				_logger.LogInformation("   • Límite Resolución: {FechaLimite} ({Minutos} min)",
					fechaLimiteResolucion, tiempoResolucionMinutos);

				// 1️⃣ PRIMERA RESPUESTA (cuando sale de "Pendiente" por primera vez)
				if (nuevoEstado != "Pendiente" && !ticket.fecha_primera_respuesta.HasValue)
				{
					ticket.fecha_primera_respuesta = ahora;
					ticket.CumplimientoRespuesta = ahora <= fechaLimiteRespuesta;

					_logger.LogInformation("📝 Primera respuesta registrada:");
					_logger.LogInformation("   • Fecha: {Fecha}", ahora);
					_logger.LogInformation("   • Cumplimiento: {Cumplido} {Emoji}",
						ticket.CumplimientoRespuesta.Value ? "SÍ" : "NO",
						ticket.CumplimientoRespuesta.Value ? "✅" : "❌");
				}

				// 2️⃣ RESOLUCIÓN (cuando llega a "Resuelto")
				if (nuevoEstado == "Resuelto" && !ticket.fecha_resolucion.HasValue)
				{
					ticket.fecha_resolucion = ahora;
					ticket.CumplimientoResolucion = ahora <= fechaLimiteResolucion;

					_logger.LogInformation("🎯 Resolución registrada:");
					_logger.LogInformation("   • Fecha: {Fecha}", ahora);
					_logger.LogInformation("   • Cumplimiento: {Cumplido} {Emoji}",
						ticket.CumplimientoResolucion.Value ? "SÍ" : "NO",
						ticket.CumplimientoResolucion.Value ? "✅" : "❌");
				}

				// 3️⃣ CIERRE (cuando llega a "Cerrado")
				if (nuevoEstado == "Cerrado")
				{
					// Marcar fecha de cierre
					if (!ticket.FechaCierre.HasValue)
					{
						ticket.FechaCierre = ahora;
						_logger.LogInformation("🔒 Ticket cerrado en: {Fecha}", ahora);
					}

					// IMPORTANTE: Si se cierra sin haber marcado resolución, considerarlo resuelto en este momento
					if (!ticket.fecha_resolucion.HasValue)
					{
						ticket.fecha_resolucion = ahora;
						ticket.CumplimientoResolucion = ahora <= fechaLimiteResolucion;

						_logger.LogInformation("🎯 Resolución registrada automáticamente (al cerrar sin pasar por Resuelto):");
						_logger.LogInformation("   • Fecha: {Fecha}", ahora);
						_logger.LogInformation("   • Límite era: {FechaLimite}", fechaLimiteResolucion);
						_logger.LogInformation("   • Diferencia: {Diff} minutos", (ahora - fechaLimiteResolucion).TotalMinutes);
						_logger.LogInformation("   • Cumplimiento: {Cumplido} {Emoji}",
							ticket.CumplimientoResolucion.Value ? "SÍ" : "NO",
							ticket.CumplimientoResolucion.Value ? "✅" : "❌");
					}
					else
					{
						_logger.LogInformation("✅ El ticket ya tenía fecha de resolución registrada: {Fecha}", ticket.fecha_resolucion.Value);
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "❌ Error al calcular cumplimiento de SLA para ticket {TicketId}", ticket.TicketId);
				// No lanzamos la excepción para no interrumpir el cambio de estado
			}
		}

		private async Task GenerarNotificacionesCambioEstadoAsync(HistorialEstadosDTO dto)
		{
			try
			{
				var ticket = await _repoTicket.FindByIdAsync(dto.TicketId);
				if (ticket == null)
				{
					_logger.LogWarning("No se encontró el ticket {TicketId} para generar notificaciones", dto.TicketId);
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

					_logger.LogInformation("📧 Notificación enviada al cliente (Usuario {UsuarioId})", ticket.UsuarioSolicitanteId);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error al crear notificación para el cliente");
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

							_logger.LogInformation("📧 Notificación enviada al técnico (Usuario {UsuarioId})", tecnico.UsuarioId);
						}
						else
						{
							_logger.LogWarning("No se encontró información del técnico {TecnicoId}", ticket.TecnicoAsignadoId.Value);
						}
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "Error al crear notificación para el técnico");
					}
				}
				else
				{
					_logger.LogInformation("El ticket no tiene técnico asignado, no se envía notificación a técnico");
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error general al generar notificaciones de cambio de estado");
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