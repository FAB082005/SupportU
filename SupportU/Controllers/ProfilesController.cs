using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SupportU.Application.DTOs;
using SupportU.Application.Services.Interfaces;

namespace SupportU.Web.Controllers
{
    [Authorize]
    public class PerfilesController : BaseController
    {
        private readonly IServiceUsuario _usuarioService;
        private readonly IServiceTicket _ticketService;
        private readonly ILogger<PerfilesController> _logger;

        public PerfilesController(
            IServiceUsuario usuarioService,
            IServiceTicket ticketService,
            ILogger<PerfilesController> logger)
        {
            _usuarioService = usuarioService;
            _ticketService = ticketService;
            _logger = logger;
        }
        // GET: /Perfiles/Index
        public async Task<IActionResult> Index()
        {
            var usuarios = await _usuarioService.ListAsync();
            // ordenar por Nombre para presentación
            var lista = usuarios?.OrderBy(u => u.Nombre).ToList() ?? new System.Collections.Generic.List<UsuarioDTO>();
            return View(lista);
        }


        // GET: /Perfiles/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0) return RedirectToAction("Index", "Home");

            var usuario = await _usuarioService.FindByIdAsync(id);
            if (usuario == null) return NotFound();

            // Obtener todos los tickets y filtrar por usuario solicitante
            var allTickets = await _ticketService.ListAsync();
            var tickets = allTickets?.Where(t => t.UsuarioSolicitanteId == id).ToList() ?? new System.Collections.Generic.List<TicketDTO>();

            ViewBag.Tickets = tickets;
            return View(usuario);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(int userId)
        {
            if (userId <= 0) return RedirectToAction("Index", "Home");

            var usuario = await _usuarioService.FindByIdAsync(userId);
            if (usuario == null) return NotFound();

            return RedirectToAction("ForgotPassword", "Login", new { email = usuario.Email });
        }
    }
}
