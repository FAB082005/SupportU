using Microsoft.Extensions.Logging;
using SupportU.Application.DTOs;
using SupportU.Application.Services.Interfaces;
using SupportU.Infraestructure.Repository.Interfaces;
using SupportU.Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupportU.Application.Services
{
	public class ServiceAutotriage : IServiceAutoTriage
	{
		private readonly IRepositoryTicket _repoTicket;
		private readonly IRepositoryTecnico _repoTecnico;
		private readonly IRepositoryCategoria _repoCategoria;
		private readonly IRepositoryAsignacion _repoAsignacion;
		private readonly IServiceNotificacion _serviceNotificacion; 
		private readonly ILogger<ServiceAutotriage> _logger;

		public ServiceAutotriage(
			IRepositoryTicket repoTicket,
			IRepositoryTecnico repoTecnico,
			IRepositoryCategoria repoCategoria,
			IRepositoryAsignacion repoAsignacion,
			IServiceNotificacion serviceNotificacion, 
			ILogger<ServiceAutotriage> logger)
		{
			_repoTicket = repoTicket;
			_repoTecnico = repoTecnico;
			_repoCategoria = repoCategoria;
			_repoAsignacion = repoAsignacion;
			_serviceNotificacion = serviceNotificacion; 
			_logger = logger;
		}

		public async Task<AutoTriageDTO> AsignarTicketAutomaticoAsync(int ticketId)
		{
			var resultado = new AutoTriageDTO { TicketId = ticketId };

			try
			{
				var ticket = await _repoTicket.FindByIdAsyncForUpdate(ticketId);
				if (ticket == null)
				{
					resultado.Exitoso = false;
					resultado.MensajeError = "Ticket no encontrado";
					return resultado;
				}

				if (ticket.Estado != "Pendiente")
				{
					resultado.Exitoso = false;
					resultado.MensajeError = $"El ticket está en estado '{ticket.Estado}', debe estar en 'Pendiente'";
					return resultado;
				}

				var categoria = await _repoCategoria.FindByIdAsync(ticket.CategoriaId);
				if (categoria?.Sla == null)
				{
					resultado.Exitoso = false;
					resultado.MensajeError = "No se encontró la categoría o SLA del ticket";
					return resultado;
				}

		

				var especialidadesRequeridas = categoria.CategoriaEspecialidades?.Select(e => e.EspecialidadId).ToList()
					?? new List<int>();

				if (!especialidadesRequeridas.Any())
				{
				
					resultado.Exitoso = false;
					resultado.MensajeError = "La categoría no tiene especialidades asociadas";
					return resultado;
				}

				var todosTecnicos = await _repoTecnico.ListAsync();

				var tecnicosDisponibles = todosTecnicos
					.Where(t => t.TecnicoId > 0 &&
								t.Estado == "Disponible" &&
								t.Especialidad != null &&
								t.Especialidad.Any(e => especialidadesRequeridas.Contains(e.EspecialidadId)))
					.ToList();


				if (!tecnicosDisponibles.Any())
				{
					resultado.Exitoso = false;
					resultado.MensajeError = "No hay técnicos disponibles con la especialidad requerida";
					return resultado;
				}

				var tiempoRestanteSLA = CalcularTiempoRestanteSLA(ticket, categoria.Sla);
				var prioridadNumerica = ObtenerPrioridadNumerica(ticket.Prioridad);

		
				var mejorTecnico = tecnicosDisponibles
					.Select(t => new
					{
						Tecnico = t,
						Puntaje = (prioridadNumerica * 1000) - tiempoRestanteSLA - (t.CargaTrabajo * 10)
					})
					.OrderByDescending(x => x.Puntaje)
					.ThenBy(x => x.Tecnico.CargaTrabajo)
					.First();

				ticket.TecnicoAsignadoId = mejorTecnico.Tecnico.TecnicoId;
				ticket.Estado = "Asignado";

				await _repoTicket.UpdateAsync(ticket);
			

				var asignacion = new Infraestructure.Models.Asignacion
				{
					TicketId = ticketId,
					TecnicoId = mejorTecnico.Tecnico.TecnicoId,
					MetodoAsignacion = "Automatico",
					FechaAsignacion = DateTime.Now,
					UsuarioAsignadorId = 1
				};
				await _repoAsignacion.AddAsync(asignacion);

				var tecnicoActualizar = await _repoTecnico.FindByUsuarioIdAsync(mejorTecnico.Tecnico.UsuarioId);
				if (tecnicoActualizar != null)
				{
					tecnicoActualizar.CargaTrabajo++;
					await _repoTecnico.UpdateAsync(tecnicoActualizar);

				}

				
				try
				{
					await _serviceNotificacion.CreateNotificationAsync(
						usuarioDestinatarioId: ticket.UsuarioSolicitanteId,
						ticketId: ticketId,
						tipo: "AsignacionTicket",
						mensaje: $"Tu ticket #{ticketId} - '{ticket.Titulo}' ha sido asignado automáticamente"
					);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error al crear notificación");
				}

				resultado.TecnicoId = mejorTecnico.Tecnico.TecnicoId;
				resultado.NombreTecnico = mejorTecnico.Tecnico.Usuario?.Nombre ?? "N/A";
				resultado.Puntaje = mejorTecnico.Puntaje;
				resultado.Justificacion = $"Asignado automáticamente: " +
					$"Prioridad={ticket.Prioridad} ({prioridadNumerica}), " +
					$"Tiempo restante SLA={tiempoRestanteSLA} min, " +
					$"Carga trabajo={mejorTecnico.Tecnico.CargaTrabajo}, " +
					$"Puntaje final={mejorTecnico.Puntaje}";
				resultado.Exitoso = true;

				_logger.LogInformation(" Autotriage completado exitosamente");
				return resultado;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, " Error en autotriage para ticket {TicketId}: {Message}", ticketId, ex.Message);
				resultado.Exitoso = false;
				resultado.MensajeError = $"Error: {ex.Message}";
				return resultado;
			}
		}

		public Task AsignarTicketPendienteAsync(int ticketId)
		{
			throw new NotImplementedException();
		}

		public async Task<List<AutoTriageDTO>> AsignarTicketsPendientesAsync()
		{
			var resultados = new List<AutoTriageDTO>();
			var todosTickets = await _repoTicket.ListAsync();
			var ticketsPendientes = todosTickets.Where(t => t.Estado == "Pendiente").ToList();


			foreach (var ticket in ticketsPendientes)
			{
				var resultado = await AsignarTicketAutomaticoAsync(ticket.TicketId);
				resultados.Add(resultado);
			}

			return resultados;
		}

		private int CalcularTiempoRestanteSLA(Infraestructure.Models.Ticket ticket, Infraestructure.Models.Sla sla)
		{
			var fechaLimite = ticket.FechaCreacion.AddMinutes(sla.TiempoResolucionMinutos);
			var tiempoRestante = (fechaLimite - DateTime.Now).TotalMinutes;
			return (int)Math.Max(0, tiempoRestante);
		}

		private int ObtenerPrioridadNumerica(string prioridad)
		{
			return prioridad switch
			{
				"Crítica" => 4,
				"Alta" => 3,
				"Media" => 2,
				"Baja" => 1,
				_ => 2
			};
		}
	}
}