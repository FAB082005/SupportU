// Controllers/HistorialEstadoController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportU.Application.DTOs;
using SupportU.Application.Services.Interfaces;
using System.Security.Claims;

namespace SupportU.Web.Controllers
{
	[Authorize]
	public class HistorialEstadosController : Controller
	{
		private readonly IServiceHistorialEstados _serviceHistorial;
		private readonly IServiceTicket _serviceTicket;
		private readonly IServiceImagen _serviceImagen;
		private readonly IWebHostEnvironment _environment;
		private readonly ILogger<HistorialEstadosController> _logger;

		public HistorialEstadosController(
			IServiceHistorialEstados serviceHistorial,
			IServiceTicket serviceTicket,
			IServiceImagen serviceImagen,
			IWebHostEnvironment environment,
			ILogger<HistorialEstadosController> logger)
		{
			_serviceHistorial = serviceHistorial;
			_serviceTicket = serviceTicket;
			_serviceImagen = serviceImagen;
			_environment = environment;
			_logger = logger;
		}

		public async Task<IActionResult> Index(int ticketId)
		{
			var historial = await _serviceHistorial.ListAsync();
			var historialTicket = historial.Where(h => h.TicketId == ticketId)
										  .OrderByDescending(h => h.FechaCambio)
										  .ToList();

			var ticket = await _serviceTicket.FindByIdAsync(ticketId);
			if (ticket == null)
			{
				TempData["Error"] = "Ticket no encontrado";
				return RedirectToAction("Index", "Ticket");
			}

			ViewBag.TicketId = ticketId;
			ViewBag.Ticket = ticket;
			return View(historialTicket);
		}

		public async Task<IActionResult> CambiarEstado(int ticketId)
		{
			var ticket = await _serviceTicket.FindByIdAsync(ticketId);
			if (ticket == null)
			{
				TempData["Error"] = "Ticket no encontrado";
				return RedirectToAction("Index", "Ticket");
			}

			if (!PuedeCambiarEstado(ticket))
			{
				TempData["Error"] = "No tiene permisos para cambiar el estado de este ticket";
				return RedirectToAction("Details", "Ticket", new { id = ticketId });
			}

			ViewBag.Ticket = ticket;
			ViewBag.EstadosSiguientes = ObtenerEstadosSiguientes(ticket.Estado);
			return View();
		}

		[HttpGet]
		public async Task<IActionResult> CambiarEstadoPartial(int ticketId = 0)
		{
			try
			{
				if (ticketId == 0)
				{
					var ticketTemporal = new TicketDTO
					{
						TicketId = 0,
						Titulo = "Nuevo Ticket",
						Estado = "Pendiente",
						FechaCreacion = DateTime.Now
					};

					ViewBag.Ticket = ticketTemporal;
					ViewBag.EstadosSiguientes = ObtenerEstadosSiguientes("Pendiente");
					return PartialView("_CambiarEstadoForm", ticketTemporal);
				}

				var ticket = await _serviceTicket.FindByIdAsync(ticketId);
				if (ticket == null)
				{
					return Content("<div class='alert alert-danger'>Ticket no encontrado</div>");
				}

				if (!PuedeCambiarEstado(ticket))
				{
					return Content("<div class='alert alert-danger'>No tiene permisos para cambiar el estado de este ticket</div>");
				}

				ViewBag.Ticket = ticket;
				ViewBag.EstadosSiguientes = ObtenerEstadosSiguientes(ticket.Estado);
				return PartialView("_CambiarEstadoForm", ticket);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error en CambiarEstadoPartial");
				return Content($"<div class='alert alert-danger'>Error al cargar el formulario: {ex.Message}</div>");
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CambiarEstado(
			int ticketId,
			string nuevoEstado,
			string observaciones,
			List<IFormFile> imagenes)
		{
			try
			{
				var usuarioId = GetCurrentUserId();
				var ticket = await _serviceTicket.FindByIdAsync(ticketId);

				if (ticket == null)
				{
					if (IsAjaxRequest())
						return Json(new { success = false, message = "Ticket no encontrado" });

					TempData["Error"] = "Ticket no encontrado";
					return RedirectToAction("Index", "Ticket");
				}

				if (!ValidarTransicionEstado(ticket.Estado, nuevoEstado))
				{
					if (IsAjaxRequest())
						return Json(new { success = false, message = "Transición de estado no válida" });

					TempData["Error"] = "Transición de estado no válida";
					return RedirectToAction("Details", "Ticket", new { id = ticketId });
				}

				if (!PuedeCambiarEstado(ticket))
				{
					if (IsAjaxRequest())
						return Json(new { success = false, message = "No tiene permisos para cambiar el estado" });

					TempData["Error"] = "No tiene permisos para cambiar el estado de este ticket";
					return RedirectToAction("Details", "Ticket", new { id = ticketId });
				}

				if (string.IsNullOrWhiteSpace(observaciones))
				{
					if (IsAjaxRequest())
						return Json(new { success = false, message = "Las observaciones son obligatorias" });

					TempData["Error"] = "Las observaciones son obligatorias";
					return RedirectToAction("CambiarEstado", new { ticketId });
				}

				if (imagenes == null || !imagenes.Any() || imagenes.All(i => i.Length == 0))
				{
					if (IsAjaxRequest())
						return Json(new { success = false, message = "Debe adjuntar al menos una imagen como evidencia" });

					TempData["Error"] = "Debe adjuntar al menos una imagen como evidencia";
					return RedirectToAction("CambiarEstado", new { ticketId });
				}

				var imagenesGuardadas = await GuardarImagenes(imagenes);

				var historialDTO = new HistorialEstadosDTO
				{
					TicketId = ticketId,
					EstadoAnterior = ticket.Estado,
					EstadoNuevo = nuevoEstado,
					Observaciones = observaciones,
					UsuarioId = usuarioId,
					FechaCambio = DateTime.Now,
					Imagenes = imagenesGuardadas
				};

				var historialId = await _serviceHistorial.AddAsync(historialDTO);

				ticket.Estado = nuevoEstado;

				if (nuevoEstado == "Resuelto")
				{
					ticket.fecha_resolucion = DateTime.Now;
				}
				else if (nuevoEstado == "Cerrado")
				{
					ticket.FechaCierre = DateTime.Now;
				}

				await _serviceTicket.UpdateAsync(ticket);

				if (IsAjaxRequest())
				{
					return Json(new
					{
						success = true,
						message = "Estado cambiado correctamente",
						nuevoEstado = nuevoEstado
					});
				}

				TempData["Success"] = "Estado cambiado correctamente";
				return RedirectToAction("Details", "Ticket", new { id = ticketId });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al cambiar estado del ticket");

				if (IsAjaxRequest())
					return Json(new { success = false, message = $"Error: {ex.InnerException?.Message ?? ex.Message}" });

				TempData["Error"] = "Error al cambiar el estado del ticket";
				return RedirectToAction("Details", "Ticket", new { id = ticketId });
			}
		}

		public async Task<IActionResult> Details(int id)
		{
			var historial = await _serviceHistorial.FindByIdAsync(id);
			if (historial == null)
			{
				TempData["Error"] = "Registro de historial no encontrado";
				return RedirectToAction("Index", "Ticket");
			}

			return View(historial);
		}

		#region Métodos Privados

		private int GetCurrentUserId()
		{
			return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
		}

		private string GetCurrentUserRol()
		{
			return User.FindFirst(ClaimTypes.Role)?.Value ?? "";
		}

		private bool PuedeCambiarEstado(TicketDTO ticket)
		{
			var rolUsuario = GetCurrentUserRol();
			var usuarioId = GetCurrentUserId();

			if (rolUsuario == "Administrador") return true;

			if (rolUsuario == "Técnico")
			{
				return ticket.TecnicoAsignadoId.HasValue &&
					   ticket.TecnicoAsignado?.UsuarioId == usuarioId;
			}

			if (rolUsuario == "Cliente")
			{
				return ticket.UsuarioSolicitanteId == usuarioId;
			}

			return false;
		}

		private bool ValidarTransicionEstado(string estadoActual, string nuevoEstado)
		{
			var transicionesValidas = new Dictionary<string, List<string>>
			{
				{ "Pendiente", new List<string> { "Asignado" } },
				{ "Asignado", new List<string> { "En Proceso" } },
				{ "En Proceso", new List<string> { "Resuelto" } },
				{ "Resuelto", new List<string> { "Cerrado" } }
			};

			return transicionesValidas.ContainsKey(estadoActual) &&
				   transicionesValidas[estadoActual].Contains(nuevoEstado);
		}

		private List<string> ObtenerEstadosSiguientes(string estadoActual)
		{
			var siguientesEstados = new Dictionary<string, List<string>>
			{
				{ "Pendiente", new List<string> { "Asignado" } },
				{ "Asignado", new List<string> { "En Proceso" } },
				{ "En Proceso", new List<string> { "Resuelto" } },
				{ "Resuelto", new List<string> { "Cerrado" } },
				{ "Cerrado", new List<string>() }
			};

			return siguientesEstados.ContainsKey(estadoActual) ?
				   siguientesEstados[estadoActual] : new List<string>();
		}

		private async Task<List<ImagenDTO>> GuardarImagenes(List<IFormFile> imagenes)
		{
			var imagenesGuardadas = new List<ImagenDTO>();
			var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "tickets");

			if (!Directory.Exists(uploadsPath))
			{
				Directory.CreateDirectory(uploadsPath);
			}

			foreach (var imagen in imagenes)
			{
				if (imagen.Length > 0)
				{
					var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(imagen.FileName)}";
					var filePath = Path.Combine(uploadsPath, fileName);

					using (var stream = new FileStream(filePath, FileMode.Create))
					{
						await imagen.CopyToAsync(stream);
					}

					imagenesGuardadas.Add(new ImagenDTO
					{
						NombreArchivo = imagen.FileName,
						RutaArchivo = $"/uploads/tickets/{fileName}",
						FechaCreacion = DateTime.Now
					});
				}
			}

			return imagenesGuardadas;
		}

		private bool IsAjaxRequest()
		{
			return Request.Headers["X-Requested-With"] == "XMLHttpRequest";
		}

		#endregion
	}
}