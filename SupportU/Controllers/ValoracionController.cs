using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using SupportU.Application.DTOs;
using SupportU.Application.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SupportU.Web.Controllers
{
	public class ValoracionController : Controller
	{
		private readonly IServiceValoracion _service;
		private readonly IServiceTicket _serviceTicket;
		private readonly ILogger<ValoracionController> _logger;

		public ValoracionController(
			IServiceValoracion service,
			IServiceTicket serviceTicket,
			ILogger<ValoracionController> logger)
		{
			_service = service;
			_serviceTicket = serviceTicket;
			_logger = logger;
		}

		private async Task CargarTicketsDisponiblesAsync()
		{
			try
			{
				// Solo mostrar tickets cerrados o resueltos sin valoración
				var tickets = await _serviceTicket.ListAsync();
				var ticketsCerrados = tickets
					.Where(t => t.Estado == "Cerrado" || t.Estado == "Resuelto")
					.ToList();

				ViewBag.Tickets = new SelectList(ticketsCerrados, "TicketId", "Titulo");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al cargar tickets");
				ViewBag.Tickets = new SelectList(Enumerable.Empty<TicketDTO>());
			}
		}

		// GET: Valoracion
		public async Task<IActionResult> Index()
		{
			try
			{
				var list = await _service.ListAsync();
				return View(list);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al listar valoraciones");
				return View(new System.Collections.Generic.List<ValoracionDTO>());
			}
		}

		// GET: Valoracion/Details/5
		public async Task<IActionResult> Details(int id)
		{
			var valoracion = await _service.GetByIdAsync(id);
			if (valoracion == null)
				return NotFound();

			return View(valoracion);
		}

		// GET: Valoracion/Create
		public async Task<IActionResult> Create(int? ticketId)
		{
			await CargarTicketsDisponiblesAsync();

			var model = new ValoracionDTO
			{
				FechaValoracion = DateTime.Now,
				Puntaje = 3 // Valor predeterminado
			};

			// Si viene un ticketId, pre-seleccionarlo y cargar info del ticket
			if (ticketId.HasValue)
			{
				model.TicketId = ticketId.Value;

				// Verificar que el ticket no tenga ya una valoración
				if (!await _service.CanCreateValoracionAsync(ticketId.Value))
				{
					TempData["Error"] = "Este ticket ya tiene una valoración";
					return RedirectToAction("Details", "Ticket", new { id = ticketId.Value });
				}

				// Obtener información del ticket para mostrar en la vista
				var ticket = await _serviceTicket.FindByIdAsync(ticketId.Value);
				if (ticket != null)
				{
					ViewBag.TicketInfo = ticket;
				}
			}

			return View(model);
		}

		// POST: Valoracion/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(ValoracionDTO dto, string returnToTicket)
		{
			// Validaciones
			if (dto.TicketId <= 0)
			{
				ModelState.AddModelError("TicketId", "Debe seleccionar un ticket");
			}

			if (dto.Puntaje < 1 || dto.Puntaje > 5)
			{
				ModelState.AddModelError("Puntaje", "El puntaje debe estar entre 1 y 5");
			}

			if (!ModelState.IsValid)
			{
				await CargarTicketsDisponiblesAsync();
				return View(dto);
			}

			try
			{
				// Obtener el UsuarioId de la sesión actual
				dto.UsuarioId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

				await _service.CreateAsync(dto);
				TempData["Success"] = "Valoración creada exitosamente";

				// Si viene desde Details de Ticket, regresar allí
				if (!string.IsNullOrEmpty(returnToTicket))
				{
					return RedirectToAction("Details", "Ticket", new { id = dto.TicketId });
				}

				return RedirectToAction(nameof(Index));
			}
			catch (InvalidOperationException ex)
			{
				ModelState.AddModelError("", ex.Message);
				await CargarTicketsDisponiblesAsync();
				return View(dto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al crear valoración");
				ModelState.AddModelError("", "Error al crear la valoración");
				await CargarTicketsDisponiblesAsync();
				return View(dto);
			}
		}

		// AJAX: Verificar si un ticket puede ser valorado
		[HttpGet]
		public async Task<IActionResult> CanCreateValoracion(int ticketId)
		{
			try
			{
				var canCreate = await _service.CanCreateValoracionAsync(ticketId);
				return Json(new { canCreate });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al verificar ticket");
				return BadRequest(new { message = ex.Message });
			}
		}
	}
}