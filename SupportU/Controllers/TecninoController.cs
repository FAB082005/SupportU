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

        // Mantengo la asignación tal como la tenías
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
            var faltantes = list.Where(x => x.TecnicoId == 0 && x.UsuarioId > 0 && x.UsuarioActivo).ToList();

            foreach (var u in faltantes)
            {
                try
                {
                    var existente = await _serviceTecnico.FindByUsuarioIdAsync(u.UsuarioId);
                    if (existente == null)
                    {
                        var nuevoDto = new TecnicoDTO
                        {
                            UsuarioId = u.UsuarioId,
                            CargaTrabajo = 0,
                            Estado = "Disponible",
                            CalificacionPromedio = 0.00m
                        };

                        await _serviceTecnico.AddAsync(nuevoDto);
                    }
                }
                catch
                {
                    // no propagamos
                }
            }

            var syncedList = await _serviceTecnico.ListAsync();
            return View(syncedList);
        }

        public async Task<IActionResult> EditByUsuario(int usuarioId)
        {
            if (usuarioId <= 0) return RedirectToAction(nameof(Index));

            var tecnicoExistente = await _serviceTecnico.FindByUsuarioIdAsync(usuarioId);
            if (tecnicoExistente != null)
            {
                return RedirectToAction(nameof(Edit), new { id = tecnicoExistente.TecnicoId });
            }

            var nuevo = new TecnicoDTO
            {
                UsuarioId = usuarioId,
                CargaTrabajo = 0,
                Estado = "Disponible",
                CalificacionPromedio = 0.00m
            };

            var newId = await _serviceTecnico.AddAsync(nuevo);
            return RedirectToAction(nameof(Edit), new { id = newId });
        }


        // GET: Tecnico/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0) return RedirectToAction(nameof(Index));

            var dto = await _serviceTecnico.FindByIdAsync(id);
            if (dto == null)
            {
                TempData["Notification"] = "Tecnico_NotFound|warning";
                return RedirectToAction(nameof(Index));
            }
             var allEspecialidades = (await _serviceEspecialidad.ListAsync())
                                     .Where(e => e.Activa)
                                     .ToList();

            ViewBag.AllEspecialidades = allEspecialidades;
            ViewBag.SelectedEspecialidades = dto.Especialidades?.Select(e => e.EspecialidadId).ToList()
                                           ?? new System.Collections.Generic.List<int>();
            return View(dto);
        }

        // POST: Tecnico/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TecnicoDTO dto, int[]? especialidades)
        {
            if (!ModelState.IsValid)
            {
                var allEspecialidades = (await _serviceEspecialidad.ListAsync()).Where(e => e.Activa).ToList();
                ViewBag.AllEspecialidades = allEspecialidades;
                ViewBag.SelectedEspecialidades = especialidades?.ToList() ?? new System.Collections.Generic.List<int>();
                return View(dto);
            }

            try
            {
                var ids = (especialidades ?? Array.Empty<int>()).ToList();
                await _serviceTecnico.UpdateEspecialidadesAsync(dto.TecnicoId, ids);

                TempData["Notification"] = "Tecnico_Updated|success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tecnico Id={Id}", dto?.TecnicoId);
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                var allEspecialidades = (await _serviceEspecialidad.ListAsync()).Where(e => e.Activa).ToList();
                ViewBag.AllEspecialidades = allEspecialidades;
                ViewBag.SelectedEspecialidades = especialidades?.ToList() ?? new System.Collections.Generic.List<int>();
                return View(dto);
            }
        }
    }
}
