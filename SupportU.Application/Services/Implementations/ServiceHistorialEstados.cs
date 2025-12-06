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

			
				if (dto.Imagenes != null && dto.Imagenes.Any())
				{
					foreach (var imagenDto in dto.Imagenes)
					{
						imagenDto.TicketId = dto.TicketId;
						imagenDto.HistorialEstadoId = historialId;

						var imagenId = await _serviceImagen.AddAsync(imagenDto);
					}

					_logger.LogInformation("Todas las imágenes guardadas exitosamente");
				}
				else
				{
					_logger.LogWarning("No se recibieron imágenes para guardar");
				}

				await GenerarNotificacionesCambioEstadoAsync(dto);
				return historialId;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "ERROR en ServiceHistorialEstados.AddAsync");
				_logger.LogError("Mensaje: {Message}", ex.Message);
				_logger.LogError("InnerException: {InnerException}", ex.InnerException?.Message);
				throw;
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

					_logger.LogInformation(" Notificación enviada al cliente (Usuario {UsuarioId})", ticket.UsuarioSolicitanteId);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error al crear notificación para el cliente");
				}

				if (ticket.TecnicoAsignadoId.HasValue)
				{
					try
					{
						// Obtener el UsuarioId del técnico
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

							_logger.LogInformation(" Notificación enviada al técnico (Usuario {UsuarioId})", tecnico.UsuarioId);
						}
						else
						{
							_logger.LogWarning(" No se encontró información del técnico {TecnicoId}", ticket.TecnicoAsignadoId.Value);
						}
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, " Error al crear notificación para el técnico");
					}
				}
				else
				{
					_logger.LogInformation("El ticket no tiene técnico asignado, no se envía notificación a técnico");
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, " Error general al generar notificaciones de cambio de estado");
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