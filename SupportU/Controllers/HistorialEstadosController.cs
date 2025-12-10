using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SupportU.Application.DTOs;
using SupportU.Application.Services.Interfaces;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SupportU.Web.Controllers
{
	[Authorize]
	public class HistorialEstadosController : BaseController
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
		private string t(string key)
		{
			var translations = ViewData["Translations"] as Dictionary<string, string>
				?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			return translations.TryGetValue(key, out var val) ? val : key;
		}

		public async Task<IActionResult> Index(int ticketId)
		{
			ViewData["Title"] = t("HistorialEstados_Index_Title");

			var historial = await _serviceHistorial.ListAsync();
			var historialTicket = historial.Where(h => h.TicketId == ticketId)
										  .OrderByDescending(h => h.FechaCambio)
										  .ToList();

			var ticket = await _serviceTicket.FindByIdAsync(ticketId);
			if (ticket == null)
			{
				TempData["Error"] = t("HistorialEstados_Error_TicketNotFound");
				return RedirectToAction("Index", "Ticket");
			}

			ViewBag.TicketId = ticketId;
			ViewBag.Ticket = ticket;
			return View(historialTicket);
		}

		public async Task<IActionResult> CambiarEstado(int ticketId)
		{
			ViewData["Title"] = t("HistorialEstados_Change_Title");

			var ticket = await _serviceTicket.FindByIdAsync(ticketId);
			if (ticket == null)
			{
				TempData["Error"] = t("HistorialEstados_Error_TicketNotFound");
				return RedirectToAction("Index", "Ticket");
			}

			if (!PuedeCambiarEstado(ticket))
			{
				TempData["Error"] = t("HistorialEstados_Error_NoPermissionChangeState");
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
						Titulo = t("HistorialEstados_Ticket_NewTitle"),
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
					return Content($"<div class='alert alert-danger'>{t("HistorialEstados_Error_TicketNotFound")}</div>");
				}

				if (!PuedeCambiarEstado(ticket))
				{
					return Content($"<div class='alert alert-danger'>{t("HistorialEstados_Error_NoPermissionChangeState")}</div>");
				}

				ViewBag.Ticket = ticket;
				ViewBag.EstadosSiguientes = ObtenerEstadosSiguientes(ticket.Estado);
				return PartialView("_CambiarEstadoForm", ticket);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error en CambiarEstadoPartial");
				return Content($"<div class='alert alert-danger'>{t("HistorialEstados_Error_LoadForm")}: {ex.Message}</div>");
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
						return Json(new { success = false, message = t("HistorialEstados_Error_TicketNotFound") });

					TempData["Error"] = t("HistorialEstados_Error_TicketNotFound");
					return RedirectToAction("Index", "Ticket");
				}

				if (!ValidarTransicionEstado(ticket.Estado, nuevoEstado))
				{
					if (IsAjaxRequest())
						return Json(new { success = false, message = t("HistorialEstados_Error_InvalidTransition") });

					TempData["Error"] = t("HistorialEstados_Error_InvalidTransition");
					return RedirectToAction("Details", "Ticket", new { id = ticketId });
				}

				if (!PuedeCambiarEstado(ticket))
				{
					if (IsAjaxRequest())
						return Json(new { success = false, message = t("HistorialEstados_Error_NoPermissionChangeState") });

					TempData["Error"] = t("HistorialEstados_Error_NoPermissionChangeState");
					return RedirectToAction("Details", "Ticket", new { id = ticketId });
				}

				if (string.IsNullOrWhiteSpace(observaciones))
				{
					if (IsAjaxRequest())
						return Json(new { success = false, message = t("HistorialEstados_Error_ObservationsRequired") });

					TempData["Error"] = t("HistorialEstados_Error_ObservationsRequired");
					return RedirectToAction("CambiarEstado", new { ticketId });
				}

				if (imagenes == null || !imagenes.Any() || imagenes.All(i => i.Length == 0))
				{
					if (IsAjaxRequest())
						return Json(new { success = false, message = t("HistorialEstados_Error_ImagesRequired") });

					TempData["Error"] = t("HistorialEstados_Error_ImagesRequired");
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

				_logger.LogInformation(" Cambio de estado completado. Ticket {TicketId}: {EstadoAnterior} → {EstadoNuevo}",
					ticketId, ticket.Estado, nuevoEstado);

				if (IsAjaxRequest())
				{
					return Json(new
					{
						success = true,
						message = t("HistorialEstados_Success_StateChanged"),
						nuevoEstado = nuevoEstado
					});
				}

				TempData["Success"] = t("HistorialEstados_Success_StateChanged");
				return RedirectToAction("Details", "Ticket", new { id = ticketId });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "❌ Error al cambiar estado del ticket");

				if (IsAjaxRequest())
					return Json(new { success = false, message = $"{t("HistorialEstados_Error_Generic")}: {ex.InnerException?.Message ?? ex.Message}" });

				TempData["Error"] = t("HistorialEstados_Error_Generic");
				return RedirectToAction("Details", "Ticket", new { id = ticketId });
			}
		}

		public async Task<IActionResult> Details(int id)
		{
			var historial = await _serviceHistorial.FindByIdAsync(id);
			if (historial == null)
			{
				TempData["Error"] = t("HistorialEstados_Error_RecordNotFound");
				return RedirectToAction("Index", "Ticket");
			}

			ViewData["Title"] = t("HistorialEstados_Details_Title");
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
				return ticket.UsuarioSolicitanteId == usuarioId && ticket.Estado == "Resuelto";
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
			var rolUsuario = GetCurrentUserRol();

			var siguientesEstados = new Dictionary<string, List<string>>
			{
				{ "Pendiente", new List<string> { "Asignado" } },
				{ "Asignado", new List<string> { "En Proceso" } },
				{ "En Proceso", new List<string> { "Resuelto" } },
				{ "Resuelto", new List<string>() },
                { "Cerrado", new List<string>() }
			};

			//  Solo Clientes y Admins ven "Cerrado" como opción en "Resuelto"
			if (estadoActual == "Resuelto" && (rolUsuario == "Cliente" || rolUsuario == "Administrador"))
			{
				siguientesEstados["Resuelto"] = new List<string> { "Cerrado" };
			}

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