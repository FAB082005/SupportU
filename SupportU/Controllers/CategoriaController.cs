using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SupportU.Application.DTOs;
using SupportU.Application.Services;

namespace SupportU.Web.Controllers
{
    public class CategoriaController : Controller
    {
        private readonly IServiceCategoria _service;
        private readonly IServiceSla _serviceSla;
        private readonly ILogger<CategoriaController> _logger;

        public CategoriaController(IServiceCategoria service, IServiceSla serviceSla, ILogger<CategoriaController> logger)
        {
            _service = service;
            _serviceSla = serviceSla;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _service.ListAsync();
            return View(list);
        }

        // GET: Create
        public async Task<IActionResult> Create()
        {
            await PopulateSlaAndAssignmentTypes(null);
            return View(new CategoriaDTO { Activa = true });
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoriaDTO dto)
        {
            _logger.LogInformation("Create POST called for Categoria. Nombre={Nombre}, SlaId={SlaId}, Criterio={Criterio}",
                dto?.Nombre, dto?.SlaId, dto?.CriterioAsignacion);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState invalid on Categoria Create. Errors: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                await PopulateSlaAndAssignmentTypes(dto);
                return View(dto);
            }

            try
            {
                var newId = await _service.AddAsync(dto);
                _logger.LogInformation("Categoria created successfully. NewId={Id}", newId);
                TempData["NotificationMessage"] = "Swal.fire('Éxito','Categoría creada correctamente','success')";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Categoria Nombre={Nombre}", dto?.Nombre);
                ModelState.AddModelError(string.Empty, $"Error al crear la categoría: {ex.Message}");
                await PopulateSlaAndAssignmentTypes(dto);
                return View(dto);
            }
        }

        // GET: Edit
        public async Task<IActionResult> Edit(int id)
        {
            var item = (await _service.ListAsync()).Find(c => c.CategoriaId == id);
            if (item == null) return NotFound();
            await PopulateSlaAndAssignmentTypes(item);
            return View(item);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoriaDTO dto)
        {
            _logger.LogInformation("Edit POST called for CategoriaId={Id}, Nombre={Nombre}", dto?.CategoriaId, dto?.Nombre);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState invalid on Categoria Edit. Id={Id}", dto?.CategoriaId);
                await PopulateSlaAndAssignmentTypes(dto);
                return View(dto);
            }

            try
            {
                await _service.UpdateAsync(dto);
                _logger.LogInformation("Categoria updated successfully. Id={Id}", dto.CategoriaId);
                TempData["NotificationMessage"] = "Swal.fire('Éxito','Categoría actualizada correctamente','success')";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Categoria Id={Id}", dto?.CategoriaId);
                ModelState.AddModelError(string.Empty, $"Error al actualizar la categoría: {ex.Message}");
                await PopulateSlaAndAssignmentTypes(dto);
                return View(dto);
            }
        }
        private async Task PopulateSlaAndAssignmentTypes(CategoriaDTO? dto)
        {
            var slas = await _serviceSla.ListAsync();
            ViewBag.Slas = new SelectList(slas, "SlaId", "Nombre", dto?.SlaId);

            var assignmentTypes = new List<SelectListItem>
            {
                new SelectListItem("Seleccionar...", "", true),
                new SelectListItem("menor_carga", "menor_carga"),
                new SelectListItem("mejor_calificado", "mejor_calificado"),
                new SelectListItem("tiempo_restante_sla", "tiempo_restante_sla"),    // priorizar por SLA restante
                new SelectListItem("prioridad_puntaje", "prioridad_puntaje"),        // combinación prioridad * factor tiempo
                new SelectListItem("especialista_disponible", "especialista_disponible"), // asignar técnico con especialidad y disponibilidad
            };
            ViewBag.AssignmentTypes = new SelectList(assignmentTypes, "Value", "Text", dto?.CriterioAsignacion);
        }
        public async Task<IActionResult> Delete(int id)
        {
            var item = (await _service.ListAsync()).Find(c => c.CategoriaId == id);
            if (item == null) return NotFound();

            return RedirectToAction(nameof(Index));
        }

        // POST: Categoria/DeleteConfirmed/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            _logger.LogInformation("DeleteConfirmed POST called for CategoriaId={Id}", id);
            try
            {
                await _service.DeleteAsync(id);
                TempData["NotificationMessage"] = "Swal.fire('Éxito','Categoría eliminada correctamente','success')";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Categoria Id={Id}", id);
                TempData["NotificationMessage"] = $"Swal.fire('Error','No se pudo eliminar la categoría: {ex.Message}','error')";
                return RedirectToAction(nameof(Index));
            }
        }

    }

}
