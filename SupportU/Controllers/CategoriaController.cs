using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using SupportU.Application.DTOs;
using SupportU.Application.Services;

namespace SupportU.Web.Controllers
{
    public class CategoriaController : BaseController
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

        private string t(string key)
        {
            var translations = ViewData["Translations"] as Dictionary<string, string>;
            if (translations != null && translations.TryGetValue(key, out var v)) return v;
            return key;
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
            _logger.LogInformation("Create POST called for Categoria. Nombre={Nombre}, SlaId={SlaId}, Criterio={Criterio}", dto?.Nombre, dto?.SlaId, dto?.CriterioAsignacion);

            if (string.IsNullOrWhiteSpace(dto.Nombre))
            {
                ModelState.AddModelError(nameof(dto.Nombre), t("Categoria_Validation_NombreRequired"));
            }

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
                TempData["NotificationMessage"] = $"Swal.fire('{t("Categoria_Create_Success")}','{t("Categoria_Create_Success")}','success')";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Categoria Nombre={Nombre}", dto?.Nombre);
                ModelState.AddModelError(string.Empty, t("Categoria_Error_Create") + ex.Message);
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

            if (string.IsNullOrWhiteSpace(dto.Nombre))
            {
                ModelState.AddModelError(nameof(dto.Nombre), t("Categoria_Validation_NombreRequired"));
            }

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
                TempData["NotificationMessage"] = $"Swal.fire('{t("Categoria_Update_Success")}','{t("Categoria_Update_Success")}','success')";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Categoria Id={Id}", dto?.CategoriaId);
                ModelState.AddModelError(string.Empty, t("Categoria_Error_Update") + ex.Message);
                await PopulateSlaAndAssignmentTypes(dto);
                return View(dto);
            }
        }

        // GET: Delete (shows confirmation)
        public async Task<IActionResult> Delete(int id)
        {
            var item = (await _service.ListAsync()).Find(c => c.CategoriaId == id);
            if (item == null) return NotFound();
            return View(item);
        }

        // POST: DeleteConfirmed (handles inactivate/reactivate via form field "estado")
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            _logger.LogInformation("DeleteConfirmed POST called for CategoriaId={Id}", id);

            var estadoForm = Request.Form["estado"].FirstOrDefault(); // "1" => inactivar, "0" => reactivar
            try
            {
                if (!string.IsNullOrEmpty(estadoForm))
                {
                    // Interpretar el estado deseado
                    bool? desiredActiva = null;
                    if (estadoForm == "1") desiredActiva = false;
                    else if (estadoForm == "0") desiredActiva = true;

                    var dto = await _service.FindByIdAsync(id);
                    if (dto != null && desiredActiva.HasValue)
                    {
                        // Actualizar el campo Activa y persistir con UpdateAsync (comportamiento igual que Usuario)
                        dto.Activa = desiredActiva.Value;
                        await _service.UpdateAsync(dto);

                        if (dto.Activa)
                            TempData["NotificationMessage"] = "Categoria_Reactivated|success";
                        else
                            TempData["NotificationMessage"] = "Categoria_Inactivated|success";
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting/reactivating Categoria Id={Id}", id);
                TempData["NotificationMessage"] = $"Swal.fire('Error','{t("Categoria_Delete_Error")} {ex.Message}','error')";
                return RedirectToAction(nameof(Index));
            }
        }


        private async Task PopulateSlaAndAssignmentTypes(CategoriaDTO? dto)
        {
            var slas = await _serviceSla.ListAsync();
            ViewBag.Slas = new SelectList(slas, "SlaId", "Nombre", dto?.SlaId);

            var translations = ViewData["Translations"] as Dictionary<string, string>
                ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string local(string key) => translations.TryGetValue(key, out var v) ? v : key;

            var assignmentTypes = new List<SelectListItem>
            {
                new SelectListItem(local("Select_Placeholder"), "", true),
                new SelectListItem(local("Assignment_MenorCarga"), "menor_carga"),
                new SelectListItem(local("Assignment_MejorCalificado"), "mejor_calificado"),
                new SelectListItem(local("Assignment_TiempoRestanteSLA"), "tiempo_restante_sla"),
                new SelectListItem(local("Assignment_PrioridadPuntaje"), "prioridad_puntaje"),
                new SelectListItem(local("Assignment_EspecialistaDisponible"), "especialista_disponible")
            };

            ViewBag.AssignmentTypes = new SelectList(assignmentTypes, "Value", "Text", dto?.CriterioAsignacion);
        }
    }
}
