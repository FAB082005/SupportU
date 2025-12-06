using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportU.Application.DTOs;
using SupportU.Application.Services;
using SupportU.Application.Services.Interfaces;
using System.Security.Claims;

namespace SupportU.Web.Controllers
{
	[Authorize]
	public class TicketController : Controller
	{
		private readonly IServiceTicket _serviceTicket;
		private readonly IServiceTecnico _serviceTecnico;
		private readonly IServiceEtiqueta _serviceEtiqueta;
		private readonly IServiceCategoria _serviceCategoria;
		private readonly IServiceAutoTriage _serviceAutotriage;
		private readonly IServiceHistorialEstados _serviceHistorial;
		private readonly ILogger<TicketController> _logger;

		public TicketController(
			IServiceTicket serviceTicket,
			IServiceTecnico serviceTecnico,
			IServiceEtiqueta serviceEtiqueta,
			IServiceCategoria serviceCategoria,
			IServiceAutoTriage serviceAutotriage,
			 IServiceHistorialEstados serviceHistorial,
			ILogger<TicketController> logger)
		{
			_serviceTicket = serviceTicket;
			_serviceTecnico = serviceTecnico;
			_serviceEtiqueta = serviceEtiqueta;
			_serviceCategoria = serviceCategoria;
			_serviceAutotriage = serviceAutotriage;
			_serviceHistorial = serviceHistorial;
			_logger = logger;
		}

		public async Task<IActionResult> Index()
		{
			var usuarioId = GetCurrentUserId();
			var rolUsuario = User.FindFirstValue(ClaimTypes.Role);

			var todosLosTickets = await _serviceTicket.ListAsync();
			IEnumerable<TicketDTO> ticketsFiltrados;

			if (rolUsuario == "Administrador")
			{
				ticketsFiltrados = todosLosTickets;
			}
			else if (rolUsuario == "Cliente")
			{
				ticketsFiltrados = todosLosTickets.Where(t => t.UsuarioSolicitanteId == usuarioId);
			}
			else if (rolUsuario == "Técnico")
			{
				var tecnicos = await _serviceTecnico.ListAsync();
				var tecnicoActual = tecnicos.FirstOrDefault(t => t.UsuarioId == usuarioId);

				ticketsFiltrados = tecnicoActual != null
					? todosLosTickets.Where(t => t.TecnicoAsignadoId == tecnicoActual.TecnicoId)
					: new List<TicketDTO>();
			}
			else
			{
				ticketsFiltrados = new List<TicketDTO>();
			}

			ViewBag.UsuarioId = usuarioId;
			ViewBag.RolUsuario = rolUsuario;
			return View(ticketsFiltrados.ToList());
		}

		public async Task<IActionResult> Details(int id)
		{
			var ticket = await _serviceTicket.FindByIdAsync(id);
			if (ticket == null) return NotFound();

			ViewBag.UsuarioId = GetCurrentUserId();
			ViewBag.RolUsuario = User.FindFirstValue(ClaimTypes.Role);
			return View(ticket);
		}

		public async Task<IActionResult> Create()
		{
			var usuarioId = GetCurrentUserId();
			await LoadViewBagData(usuarioId);

			var dto = new TicketDTO
			{
				UsuarioSolicitanteId = usuarioId,
				Estado = "Pendiente",
				FechaCreacion = DateTime.Now,
				Prioridad = "Media"
			};

			return View(dto);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(TicketDTO dto, int? etiquetaSeleccionadaId)
		{
			var usuarioId = GetCurrentUserId();

			if (!etiquetaSeleccionadaId.HasValue || etiquetaSeleccionadaId.Value <= 0)
			{
				ModelState.AddModelError("", "Debe seleccionar una etiqueta");
				await LoadViewBagData(usuarioId);
				return View(dto);
			}

			if (!ModelState.IsValid)
			{
				await LoadViewBagData(usuarioId);
				return View(dto);
			}

			try
			{
				var ticketDTO = new TicketDTO
				{
					Titulo = dto.Titulo,
					Descripcion = dto.Descripcion,
					CategoriaId = dto.CategoriaId,
					UsuarioSolicitanteId = usuarioId,
					Prioridad = dto.Prioridad,
					FechaCreacion = DateTime.Now,
					Estado = "Pendiente", 
					TecnicoAsignadoId = null
				};

				var newTicketId = await _serviceTicket.AddAsync(ticketDTO);

				TempData["NotificationMessage"] = "Swal.fire('Éxito', 'Ticket creado correctamente. Un administrador lo asignará pronto.', 'success')";
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al crear ticket");
				ModelState.AddModelError(string.Empty, $"Error al crear el ticket: {ex.Message}");
				await LoadViewBagData(usuarioId);
				return View(dto);
			}
		}

		[HttpPost]
		[Authorize(Roles = "Administrador")]
		public async Task<IActionResult> AsignarAutomatico([FromBody] AsignarTicketRequest request)
		{
			try
			{
				_logger.LogInformation(" [CONTROLLER] AsignarAutomatico iniciado");

				if (request == null || request.ticketId <= 0)
				{
					return Json(new { success = false, message = "ID de ticket inválido" });
				}

				var ticketId = request.ticketId;

				var ticket = await _serviceTicket.FindByIdAsync(ticketId);
				if (ticket == null)
				{
					return Json(new { success = false, message = "Ticket no encontrado" });
				}
				var resultado = await _serviceAutotriage.AsignarTicketAutomaticoAsync(ticketId);

				if (resultado.Exitoso)
				{
				
					var historialAsignacion = new HistorialEstadosDTO
					{
						TicketId = ticketId,
						EstadoAnterior = "Pendiente",
						EstadoNuevo = "Asignado",
						UsuarioId = GetCurrentUserId(), // Admin que ejecutó
						Observaciones = $"Asignado automáticamente por autotriage al técnico {resultado.NombreTecnico}. Justificación: {resultado.Justificacion}",
						FechaCambio = DateTime.Now,
						Imagenes = new List<ImagenDTO>() // Sin imágenes en asignación auto
					};

					await _serviceHistorial.AddAsync(historialAsignacion);

					return Json(new
					{
						success = true,
						message = $"Técnico asignado: {resultado.NombreTecnico}",
						tecnico = resultado.NombreTecnico
					});
				}
				else
				{
					return Json(new { success = false, message = resultado.MensajeError });
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "💥 [CONTROLLER] Error en AsignarAutomatico");
				return Json(new { success = false, message = $"Error: {ex.Message}" });
			}
		}


		public class AsignarTicketRequest
		{
			public int ticketId { get; set; }
		}


		[HttpPost]
		[Authorize(Roles = "Administrador")]
		public async Task<IActionResult> AsignarManual([FromBody] AsignarManualRequest request)
		{
			try
			{
				_logger.LogInformation("🎯 Asignación manual iniciada. Ticket: {TicketId}, Técnico: {TecnicoId}",
					request?.ticketId ?? 0, request?.tecnicoId ?? 0);

				if (request == null || request.ticketId <= 0 || request.tecnicoId <= 0)
				{
					return Json(new { success = false, message = "Datos inválidos" });
				}

				var ticket = await _serviceTicket.FindByIdAsync(request.ticketId);
				if (ticket == null)
				{
					return Json(new { success = false, message = "Ticket no encontrado" });
				}

				if (ticket.Estado != "Pendiente")
				{
					return Json(new { success = false, message = "El ticket no está en estado Pendiente" });
				}

				// Obtener técnico
				var tecnico = await _serviceTecnico.FindByIdAsync(request.tecnicoId);
				if (tecnico == null)
				{
					return Json(new { success = false, message = "Técnico no encontrado" });
				}

				// Actualizar ticket
				ticket.TecnicoAsignadoId = request.tecnicoId;
				ticket.Estado = "Asignado";
				await _serviceTicket.UpdateAsync(ticket);

				//  CREAR HISTORIAL DE ASIGNACIÓN MANUAL
				var historialAsignacion = new HistorialEstadosDTO
				{
					TicketId = request.ticketId,
					EstadoAnterior = "Pendiente",
					EstadoNuevo = "Asignado",
					UsuarioId = GetCurrentUserId(), // Admin que asignó
					Observaciones = $"Asignado manualmente al técnico {tecnico.NombreUsuario} por el administrador",
					FechaCambio = DateTime.Now,
					Imagenes = new List<ImagenDTO>() // Sin imágenes en asignación manual
				};

				await _serviceHistorial.AddAsync(historialAsignacion);

				_logger.LogInformation("✅ Ticket {TicketId} asignado manualmente a técnico {TecnicoId}",
					request.ticketId, request.tecnicoId);

				return Json(new
				{
					success = true,
					message = "Asignación manual exitosa",
					tecnico = tecnico.NombreUsuario
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error en asignación manual");
				return Json(new { success = false, message = $"Error: {ex.Message}" });
			}
		}

		// CLASE PARA AsignarManual (dentro del namespace, fuera de la clase)
		public class AsignarManualRequest
		{
			public int ticketId { get; set; }
			public int tecnicoId { get; set; }
		}

		[HttpGet]
		[Authorize(Roles = "Administrador")]
		public async Task<IActionResult> ObtenerTecnicosDisponibles(int categoriaId)
		{
			try
			{
				_logger.LogInformation("Obteniendo técnicos disponibles para categoría {CategoriaId}", categoriaId);

				var tecnicos = await _serviceTecnico.ListAsync();

				var tecnicosDisponibles = tecnicos
					.Where(t => t.Estado == "Disponible")
					.Select(t => new {
						id = t.TecnicoId,
						nombre = t.NombreUsuario,
						carga = t.CargaTrabajo,
						estado = t.Estado,
						correo = t.CorreoUsuario,
						calificacion = t.CalificacionPromedio,
						especialidades = t.Especialidades != null
							? string.Join(", ", t.Especialidades.Where(e => e.Activa).Select(e => e.Nombre))
							: "Sin especialidades"
					})
					.OrderBy(t => t.carga)
					.ToList();

				return Json(tecnicosDisponibles);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al obtener técnicos disponibles");
				return Json(new List<object>());
			}
		}

		public async Task<IActionResult> Edit(int id)
		{
			var ticket = await _serviceTicket.FindByIdAsync(id);
			if (ticket == null) return NotFound();

			var usuarioId = GetCurrentUserId();
			await LoadViewBagData(usuarioId);

			return View(ticket);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(
			TicketDTO dto,
			string metodoAsignacion,
			int? tecnicoSeleccionadoId,
			int? etiquetaSeleccionadaId)
		{
			var usuarioId = GetCurrentUserId();

			if (!ValidarDatosTicket(etiquetaSeleccionadaId, metodoAsignacion, tecnicoSeleccionadoId))
			{
				await LoadViewBagData(usuarioId);
				return View(dto);
			}

			if (!ModelState.IsValid)
			{
				await LoadViewBagData(usuarioId);
				return View(dto);
			}

			try
			{
				ActualizarAsignacionTecnico(dto, metodoAsignacion, tecnicoSeleccionadoId);
				await _serviceTicket.UpdateAsync(dto);

				_logger.LogInformation("Ticket actualizado. Id={Id}", dto.TicketId);
				TempData["NotificationMessage"] = "Swal.fire('Éxito','Ticket actualizado correctamente','success')";

				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al actualizar ticket Id={Id}", dto?.TicketId);
				ModelState.AddModelError(string.Empty, $"Error al actualizar el ticket: {ex.Message}");
				await LoadViewBagData(usuarioId);
				return View(dto);
			}
		}

		#region Métodos Privados

		private int GetCurrentUserId()
		{
			return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
		}

		private async Task LoadViewBagData(int usuarioId)
		{
			var tecnicos = await _serviceTecnico.ListAsync();
			var etiquetas = await _serviceEtiqueta.ListAsync();
			var categorias = await _serviceCategoria.ListAsync();

			ViewBag.Tecnicos = tecnicos.Where(t => t.Estado == "Disponible").ToList();
			ViewBag.Etiquetas = etiquetas.Where(e => e.Activa).ToList();
			ViewBag.Categorias = categorias.Where(c => c.Activa).ToList();
			ViewBag.UsuarioId = usuarioId;
		}

		private bool ValidarDatosTicket(int? etiquetaId, string metodoAsignacion, int? tecnicoId)
		{
			bool esValido = true;

			if (!etiquetaId.HasValue || etiquetaId.Value <= 0)
			{
				ModelState.AddModelError("", "Debe seleccionar una etiqueta");
				esValido = false;
			}

			if (metodoAsignacion == "Manual" && (!tecnicoId.HasValue || tecnicoId.Value <= 0))
			{
				ModelState.AddModelError("", "Debe seleccionar un técnico para asignación manual");
				esValido = false;
			}

			return esValido;
		}

		private void LogModelStateErrors()
		{
			_logger.LogWarning("ModelState inválido");
			foreach (var key in ModelState.Keys)
			{
				var state = ModelState[key];
				if (state.Errors.Count > 0)
				{
					foreach (var error in state.Errors)
					{
						_logger.LogError("Campo: {Key} - Error: {Error}", key, error.ErrorMessage);
					}
				}
			}
		}

		private void ActualizarAsignacionTecnico(TicketDTO dto, string metodoAsignacion, int? tecnicoId)
		{
			if (metodoAsignacion == "Manual" && tecnicoId.HasValue && tecnicoId.Value > 0)
			{
				dto.TecnicoAsignadoId = tecnicoId.Value;
				if (dto.Estado == "Pendiente")
				{
					dto.Estado = "Asignado";
				}
				_logger.LogInformation("Asignación manual al técnico ID: {TecnicoId}", tecnicoId.Value);
			}
			else if (metodoAsignacion == "Automatico")
			{
				dto.TecnicoAsignadoId = null;
				dto.Estado = "Pendiente";
				_logger.LogInformation("Cambiado a asignación automática");
			}
		}

		#endregion
	}
}