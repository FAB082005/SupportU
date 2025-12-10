using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportU.Application.DTOs;
using SupportU.Application.Services;
using SupportU.Application.Services.Implementations;
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
		private readonly IServiceAsignacion _serviceAsignacion;
		private readonly ILogger<TicketController> _logger;

		public TicketController(
			IServiceTicket serviceTicket,
			IServiceTecnico serviceTecnico,
			IServiceEtiqueta serviceEtiqueta,
			IServiceCategoria serviceCategoria,
			IServiceAutoTriage serviceAutotriage,
			 IServiceHistorialEstados serviceHistorial,
			 IServiceAsignacion serviceAsignacion,
			ILogger<TicketController> logger)
		{
			_serviceTicket = serviceTicket;
			_serviceTecnico = serviceTecnico;
			_serviceEtiqueta = serviceEtiqueta;
			_serviceCategoria = serviceCategoria;
			_serviceAutotriage = serviceAutotriage;
			_serviceHistorial = serviceHistorial;
			_serviceAsignacion = serviceAsignacion;
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
			try
			{
				var ticket = await _serviceTicket.FindByIdAsync(id);
				if (ticket == null) return NotFound();

				// IMPORTANTE: Cargar la valoración si existe
				// Agregar esto si no lo tienes en tu servicio
				ViewBag.UsuarioId = GetCurrentUserId();
				ViewBag.RolUsuario = User.FindFirstValue(ClaimTypes.Role);

				return View(ticket);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al obtener detalles del ticket {TicketId}", id);
				TempData["Error"] = "Error al cargar el ticket";
				return RedirectToAction(nameof(Index));
			}
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

				// ✅ ACTUALIZAR O CREAR ASIGNACIÓN CON EL TÉCNICO ASIGNADO
				var todasAsignaciones = await _serviceAsignacion.ListAsync();
				var asignacionExistente = todasAsignaciones.FirstOrDefault(a => a.TicketId == request.ticketId);

				if (asignacionExistente != null)
				{
					// Si ya existe una asignación, actualizarla
					asignacionExistente.TecnicoId = request.tecnicoId;
					asignacionExistente.MetodoAsignacion = "Manual";
					asignacionExistente.FechaAsignacion = DateTime.Now;
					asignacionExistente.UsuarioAsignadorId = GetCurrentUserId();
				}
				else
				{
					// Si no existe, crear nueva asignación
					var nuevaAsignacion = new AsignacionDTO
					{
						TicketId = request.ticketId,
						TecnicoId = request.tecnicoId,
						MetodoAsignacion = "Manual",
						FechaAsignacion = DateTime.Now,
						UsuarioAsignadorId = GetCurrentUserId()
					};

					await _serviceAsignacion.AddAsync(nuevaAsignacion);
				}

				// Crear historial de asignación manual
				var historialAsignacion = new HistorialEstadosDTO
				{
					TicketId = request.ticketId,
					EstadoAnterior = "Pendiente",
					EstadoNuevo = "Asignado",
					UsuarioId = GetCurrentUserId(),
					Observaciones = $"Asignado manualmente al técnico {tecnico.NombreUsuario} por el administrador",
					FechaCambio = DateTime.Now,
					Imagenes = new List<ImagenDTO>()
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

		public class AsignarTicketRequest
		{
			public int ticketId { get; set; }
		}


		[HttpPost]
		[Authorize(Roles = "Administrador")]
		public async Task<IActionResult> AsignarAutomatico([FromBody] AsignarTicketRequest request)
		{
			try
			{
				_logger.LogInformation("🤖 [CONTROLLER] AsignarAutomatico iniciado");

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
					// ✅ ACTUALIZAR O CREAR ASIGNACIÓN CON EL TÉCNICO ASIGNADO
					var todasAsignaciones = await _serviceAsignacion.ListAsync();
					var asignacionExistente = todasAsignaciones.FirstOrDefault(a => a.TicketId == ticketId);

					if (asignacionExistente != null)
					{
						// Si ya existe una asignación, actualizarla
						asignacionExistente.TecnicoId = resultado.TecnicoId;
						asignacionExistente.MetodoAsignacion = "Automatico";
						asignacionExistente.FechaAsignacion = DateTime.Now;
						asignacionExistente.UsuarioAsignadorId = GetCurrentUserId();
					}
					else
					{
						// Si no existe, crear nueva asignación
						var nuevaAsignacion = new AsignacionDTO
						{
							TicketId = ticketId,
							TecnicoId = resultado.TecnicoId,
							MetodoAsignacion = "Automatico",
							FechaAsignacion = DateTime.Now,
							UsuarioAsignadorId = GetCurrentUserId()
						};

						await _serviceAsignacion.AddAsync(nuevaAsignacion);
					}

					// Crear historial
					var historialAsignacion = new HistorialEstadosDTO
					{
						TicketId = ticketId,
						EstadoAnterior = "Pendiente",
						EstadoNuevo = "Asignado",
						UsuarioId = GetCurrentUserId(),
						Observaciones = $"Asignado automáticamente por autotriage al técnico {resultado.NombreTecnico}. Justificación: {resultado.Justificacion}",
						FechaCambio = DateTime.Now,
						Imagenes = new List<ImagenDTO>()
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

		[HttpGet]
		public async Task<IActionResult> Edit(int id)
		{
			try
			{
				var ticket = await _serviceTicket.FindByIdAsync(id);
				if (ticket == null)
				{
					TempData["Error"] = "Ticket no encontrado";
					return RedirectToAction(nameof(Index));
				}

				var usuarioId = GetCurrentUserId();
				var rolUsuario = User.FindFirstValue(ClaimTypes.Role);

				// Solo permitir edición si es admin, el dueño o técnico asignado
				if (rolUsuario != "Administrador" &&
					ticket.UsuarioSolicitanteId != usuarioId &&
					ticket.TecnicoAsignadoId != usuarioId)
				{
					TempData["Error"] = "No tiene permisos para editar este ticket";
					return RedirectToAction(nameof(Index));
				}

				await LoadViewBagData(usuarioId);

				// Obtener la etiqueta actual basada en la categoría del ticket
				if (ticket.CategoriaId > 0)
				{
					// Buscar etiquetas que pertenecen a esta categoría
					var etiquetas = await _serviceEtiqueta.ListAsync();
					var etiquetasDeCategoria = etiquetas.Where(e => e.CategoriaId == ticket.CategoriaId).ToList();

					if (etiquetasDeCategoria.Any())
					{
						// Si solo hay una etiqueta para esta categoría, seleccionarla
						if (etiquetasDeCategoria.Count == 1)
						{
							ViewBag.EtiquetaSeleccionada = etiquetasDeCategoria.First().EtiquetaId;
						}
						// Si hay múltiples, necesitarías lógica adicional para determinar cuál es la correcta
					}
				}

				return View(ticket);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al cargar ticket para editar. Id: {Id}", id);
				TempData["Error"] = "Error al cargar el ticket";
				return RedirectToAction(nameof(Index));
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(TicketDTO dto, int etiquetaSeleccionadaId)
		{
			try
			{
				_logger.LogInformation("Edit POST - Iniciando edición del ticket {TicketId}", dto.TicketId);

				// Validar que el ticket existe
				var ticketExistente = await _serviceTicket.FindByIdAsync(dto.TicketId);
				if (ticketExistente == null)
				{
					TempData["Error"] = "Ticket no encontrado";
					return RedirectToAction(nameof(Index));
				}

				// Verificar permisos
				var usuarioId = GetCurrentUserId();
				var rolUsuario = User.FindFirstValue(ClaimTypes.Role);

				if (rolUsuario != "Administrador" &&
					ticketExistente.UsuarioSolicitanteId != usuarioId &&
					ticketExistente.TecnicoAsignadoId != usuarioId)
				{
					TempData["Error"] = "No tiene permisos para editar este ticket";
					return RedirectToAction(nameof(Index));
				}

				// Validar datos requeridos
				if (etiquetaSeleccionadaId <= 0)
				{
					ModelState.AddModelError("", "Debe seleccionar una etiqueta");
					await LoadViewBagData(usuarioId);
					return View(ticketExistente);
				}

				// Obtener la etiqueta seleccionada para determinar la categoría
				var etiquetas = await _serviceEtiqueta.ListAsync();
				var etiquetaSeleccionada = etiquetas.FirstOrDefault(e => e.EtiquetaId == etiquetaSeleccionadaId);

				if (etiquetaSeleccionada == null)
				{
					ModelState.AddModelError("", "Etiqueta seleccionada no válida");
					await LoadViewBagData(usuarioId);
					return View(ticketExistente);
				}

				// Asignar la nueva categoría basada en la etiqueta seleccionada
				dto.CategoriaId = etiquetaSeleccionada.CategoriaId;

				_logger.LogInformation("Edit POST - Etiqueta seleccionada: {EtiquetaId} -> Categoría: {CategoriaId}",
					etiquetaSeleccionadaId, dto.CategoriaId);

				// Mantener campos que no se editan directamente en el formulario
				dto.UsuarioSolicitanteId = ticketExistente.UsuarioSolicitanteId;
				dto.FechaCreacion = ticketExistente.FechaCreacion;
				dto.Estado = ticketExistente.Estado;
				dto.FechaCierre = ticketExistente.FechaCierre;
				dto.CumplimientoRespuesta = ticketExistente.CumplimientoRespuesta;
				dto.CumplimientoResolucion = ticketExistente.CumplimientoResolucion;
				dto.fecha_primera_respuesta = ticketExistente.fecha_primera_respuesta;
				dto.fecha_resolucion = ticketExistente.fecha_resolucion;
				dto.TecnicoAsignadoId = ticketExistente.TecnicoAsignadoId;

				// Validar ModelState
				if (!ModelState.IsValid)
				{
					_logger.LogWarning("Edit POST - ModelState inválido. Errores:");
					foreach (var key in ModelState.Keys)
					{
						var state = ModelState[key];
						if (state.Errors.Count > 0)
						{
							_logger.LogWarning("  {Key}: {Error}", key,
								string.Join(", ", state.Errors.Select(e => e.ErrorMessage)));
						}
					}

					await LoadViewBagData(usuarioId);
					return View(ticketExistente);
				}

				// Actualizar el ticket
				await _serviceTicket.UpdateAsync(dto);
				_logger.LogInformation("Edit POST - Ticket {TicketId} actualizado correctamente", dto.TicketId);

				TempData["Success"] = "Ticket actualizado correctamente";
				return RedirectToAction(nameof(Details), new { id = dto.TicketId });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al actualizar ticket Id={TicketId}", dto?.TicketId);
				TempData["Error"] = $"Error al actualizar el ticket: {ex.Message}";

				var usuarioId = GetCurrentUserId();
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