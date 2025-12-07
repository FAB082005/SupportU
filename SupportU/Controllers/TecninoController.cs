using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SupportU.Application.DTOs;
using SupportU.Application.Services;

namespace SupportU.Web.Controllers
{
    public class TecnicoController : BaseController
    {
        private readonly IServiceTecnico _serviceTecnico;
        private readonly IServiceEspecialidad _serviceEspecialidad;
        private readonly ILogger<TecnicoController> _logger;

        public TecnicoController(IServiceTecnico serviceTecnico, IServiceEspecialidad serviceEspecialidad, ILogger<TecnicoController> logger)
        {
            _serviceTecnico = serviceTecnico;
            _serviceEspecialidad = serviceEspecialidad;
            _logger = logger;
        }

        // GET: Tecnico
        public async Task<IActionResult> Index()
        {
            var list = await _serviceTecnico.ListAsync();
            return View(list);
        }

        // GET: Tecnico/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var dto = await _serviceTecnico.FindByIdAsync(id);
            if (dto == null) return NotFound();

            var allEspecialidades = await _serviceEspecialidad.ListAsync();
            ViewBag.AllEspecialidades = allEspecialidades; // lista de EspecialidadDTO
            ViewBag.SelectedEspecialidades = dto.Especialidades?.Select(e => e.EspecialidadId).ToList() ?? new System.Collections.Generic.List<int>();

            return View(dto);
        }

        // POST: Tecnico/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TecnicoDTO dto, int[]? especialidades)
        {
            if (!ModelState.IsValid)
            {
                var allEspecialidades = await _serviceEspecialidad.ListAsync();
                ViewBag.AllEspecialidades = allEspecialidades;
                ViewBag.SelectedEspecialidades = especialidades?.ToList() ?? new System.Collections.Generic.List<int>();
                return View(dto);
            }

            try
            {
                var ids = (especialidades ?? Array.Empty<int>()).ToList();
                await _serviceTecnico.UpdateEspecialidadesAsync(dto.TecnicoId, ids);

                TempData["NotificationMessage"] = "Swal.fire('Éxito','Técnico actualizado correctamente','success')";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tecnico Id={Id}", dto?.TecnicoId);
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                var allEspecialidades = await _serviceEspecialidad.ListAsync();
                ViewBag.AllEspecialidades = allEspecialidades;
                ViewBag.SelectedEspecialidades = especialidades?.ToList() ?? new System.Collections.Generic.List<int>();
                return View(dto);
            }
        }
    }
}
