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
        private readonly ILogger<TicketController> _logger;

        public TicketController(
            IServiceTicket serviceTicket,
            IServiceTecnico serviceTecnico,
            IServiceEtiqueta serviceEtiqueta,
            IServiceCategoria serviceCategoria,
            ILogger<TicketController> logger)
        {
            _serviceTicket = serviceTicket;
            _serviceTecnico = serviceTecnico;
            _serviceEtiqueta = serviceEtiqueta;
            _serviceCategoria = serviceCategoria;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
          
            var usuarioId = GetCurrentUserId();
            var rolUsuario = User.FindFirstValue(ClaimTypes.Role); // Obtener el rol del usuario

            // Traer todos los tickets
            var todosLosTickets = await _serviceTicket.ListAsync();

            IEnumerable<TicketDTO> ticketsFiltrados;

            if (rolUsuario == "Administrador")
            {
                // De esta forma el admin ve todos los tiquetes
                ticketsFiltrados = todosLosTickets;
            }
            else if (rolUsuario == "Cliente")
            {
                // El cliente solo ve los de el
                ticketsFiltrados = todosLosTickets.Where(t => t.UsuarioSolicitanteId == usuarioId);
            }
            else if (rolUsuario == "Técnico")
            {

                
                var tecnicos = await _serviceTecnico.ListAsync();
                var tecnicoActual = tecnicos.FirstOrDefault(t => t.UsuarioId == usuarioId);

                if (tecnicoActual != null)
                {
                    // Filtrar tickets asignados a este técnico
                    ticketsFiltrados = todosLosTickets.Where(t => t.TecnicoAsignadoId == tecnicoActual.TecnicoId);
                }
                else
                {
                    // Si no existe como técnico, no ve ningún ticket
                    ticketsFiltrados = new List<TicketDTO>();
                }
            }
            else
            {
                // Rol no reconocido
                ticketsFiltrados = new List<TicketDTO>();
            }

            ViewBag.UsuarioId = usuarioId;
            ViewBag.RolUsuario = rolUsuario; 

            return View(ticketsFiltrados.ToList());
        }

        public async Task<IActionResult> Details(int id)
        {
            var ticket = await _serviceTicket.FindByIdAsync(id);
            if (ticket == null)
                return NotFound();

            ViewBag.UsuarioId = GetCurrentUserId();
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
        public async Task<IActionResult> Create(
            TicketDTO dto,
            string metodoAsignacion,
            int? tecnicoSeleccionadoId,
            int? etiquetaSeleccionadaId)
        {
            var usuarioId = GetCurrentUserId();

            _logger.LogInformation("Creando ticket: {Titulo}, Método: {Metodo}",
                dto?.Titulo, metodoAsignacion);

            if (!ValidarDatosTicket(etiquetaSeleccionadaId, metodoAsignacion, tecnicoSeleccionadoId))
            {
                await LoadViewBagData(usuarioId);
                return View(dto);
            }

            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                await LoadViewBagData(usuarioId);
                return View(dto);
            }

            try
            {
                var ticketDTO = ConstruirTicketDTO(dto, usuarioId, metodoAsignacion, tecnicoSeleccionadoId);
                var newId = await _serviceTicket.AddAsync(ticketDTO);

                _logger.LogInformation("Ticket creado con ID: {Id}", newId);
                TempData["NotificationMessage"] = "Swal.fire('Éxito','Ticket creado correctamente','success')";

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

        public async Task<IActionResult> Edit(int id)
        {
            var ticket = await _serviceTicket.FindByIdAsync(id);
            if (ticket == null)
                return NotFound();

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
            int? etiquetaSeleccionadaId,
            string observacionesHistorial,
            bool agregarHistorial = false)
        {
            var usuarioId = GetCurrentUserId();

            _logger.LogInformation("Editando ticket ID: {Id}, Agregar historial: {Flag}",
                dto?.TicketId, agregarHistorial);

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
                var ticketAnterior = await _serviceTicket.FindByIdAsync(dto.TicketId);
                string estadoAnterior = ticketAnterior?.Estado ?? dto.Estado;

                ActualizarAsignacionTecnico(dto, metodoAsignacion, tecnicoSeleccionadoId);
                await _serviceTicket.UpdateAsync(dto);

                if (agregarHistorial && !string.IsNullOrWhiteSpace(observacionesHistorial))
                {
                    await CrearHistorialEstados(dto.TicketId, estadoAnterior, dto.Estado,
                        observacionesHistorial, usuarioId);
                }

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

        private TicketDTO ConstruirTicketDTO(TicketDTO dto, int usuarioId, string metodoAsignacion, int? tecnicoId)
        {
            var ticketDTO = new TicketDTO
            {
                Titulo = dto.Titulo,
                Descripcion = dto.Descripcion,
                CategoriaId = dto.CategoriaId,
                UsuarioSolicitanteId = usuarioId,
                Prioridad = dto.Prioridad,
                FechaCreacion = DateTime.Now
            };

            if (metodoAsignacion == "Manual" && tecnicoId.HasValue && tecnicoId.Value > 0)
            {
                ticketDTO.TecnicoAsignadoId = tecnicoId.Value;
                ticketDTO.Estado = "Asignado";
                _logger.LogInformation("Asignación manual al técnico ID: {TecnicoId}", tecnicoId.Value);
            }
            else
            {
                ticketDTO.TecnicoAsignadoId = null;
                ticketDTO.Estado = "Pendiente";
                _logger.LogInformation("Asignación automática - Técnico será asignado después");
            }

            return ticketDTO;
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

        private async Task CrearHistorialEstados(int ticketId, string estadoAnterior,
            string estadoNuevo, string observaciones, int usuarioId)
        {
            try
            {
                var historialDTO = new HistorialEstadosDTO
                {
                    TicketId = ticketId,
                    EstadoAnterior = estadoAnterior,
                    EstadoNuevo = estadoNuevo,
                    Observaciones = observaciones,
                    FechaCambio = DateTime.Now,
                    UsuarioId = usuarioId
                };

                var serviceHistorial = HttpContext.RequestServices.GetService<IServiceHistorialEstados>();
                if (serviceHistorial != null)
                {
                    await serviceHistorial.AddAsync(historialDTO);
                    _logger.LogInformation("Historial de estados creado para Ticket #{Id}", ticketId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo crear el historial, pero el ticket se actualizó correctamente");
            }
        }
    }
}