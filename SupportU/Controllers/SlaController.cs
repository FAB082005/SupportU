using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SupportU.Application.DTOs;
using SupportU.Application.Services;

namespace SupportU.Web.Controllers
{
    public class SlaController : BaseController
    {
        private readonly IServiceSla _service;
        private readonly ILogger<SlaController> _logger;

        public SlaController(IServiceSla service, ILogger<SlaController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private string t(string key)
        {
            var translations = ViewData["Translations"] as System.Collections.Generic.Dictionary<string, string>;
            if (translations != null && translations.TryGetValue(key, out var v)) return v;
            return key;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = t("Sla_Title_Index");
            var list = await _service.ListAsync();
            return View(list);
        }

        public async Task<IActionResult> Details(int id)
        {
            ViewData["Title"] = t("Sla_Title_Index");
            var dto = await _service.FindByIdAsync(id);
            if (dto == null) return NotFound();
            return View(dto);
        }

        public IActionResult Create()
        {
            ViewData["Title"] = t("Sla_Title_Create");
            return View(new SlaDTO { Activo = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SlaDTO dto)
        {
            ViewData["Title"] = t("Sla_Title_Create");

            if (string.IsNullOrWhiteSpace(dto.Nombre))
                ModelState.AddModelError(nameof(dto.Nombre), t("Sla_Validation_NombreRequired"));

            if (dto.TiempoRespuestaMinutos <= 0)
                ModelState.AddModelError(nameof(dto.TiempoRespuestaMinutos), t("Sla_Validation_TiempoRespuestaPositive"));

            if (dto.TiempoResolucionMinutos <= 0)
                ModelState.AddModelError(nameof(dto.TiempoResolucionMinutos), t("Sla_Validation_TiempoResolucionPositive"));

            if (!ModelState.IsValid) return View(dto);

            try
            {
                await _service.AddAsync(dto);
                TempData["NotificationMessage"] = $"Swal.fire('{t("Sla_Notification_Created")}','{t("Sla_Notification_Created")}','success')";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                TempData["LastDbError"] = ex.Message;
                return View(dto);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Title"] = t("Sla_Title_Edit");
            var dto = await _service.FindByIdAsync(id);
            if (dto == null) return NotFound();
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SlaDTO dto)
        {
            ViewData["Title"] = t("Sla_Title_Edit");

            if (string.IsNullOrWhiteSpace(dto.Nombre))
                ModelState.AddModelError(nameof(dto.Nombre), t("Sla_Validation_NombreRequired"));

            if (dto.TiempoRespuestaMinutos <= 0)
                ModelState.AddModelError(nameof(dto.TiempoRespuestaMinutos), t("Sla_Validation_TiempoRespuestaPositive"));

            if (dto.TiempoResolucionMinutos <= 0)
                ModelState.AddModelError(nameof(dto.TiempoResolucionMinutos), t("Sla_Validation_TiempoResolucionPositive"));

            if (!ModelState.IsValid) return View(dto);

            try
            {
                await _service.UpdateAsync(dto);
                TempData["NotificationMessage"] = $"Swal.fire('{t("Sla_Notification_Updated")}','{t("Sla_Notification_Updated")}','success')";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                TempData["LastDbError"] = ex.Message;
                return View(dto);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var dto = await _service.FindByIdAsync(id);
            if (dto == null) return NotFound();
            ViewData["Title"] = dto.Activo ? t("Sla_Title_Delete_Inactivate") : t("Sla_Title_Delete_Reactivate");
            return View(dto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var estadoForm = Request.Form["estado"].FirstOrDefault();
            try
            {
                if (!string.IsNullOrEmpty(estadoForm))
                {
                    var dto = await _service.FindByIdAsync(id);
                    if (dto != null)
                    {
                        if (estadoForm == "1")
                        {
                            dto.Activo = false;
                            await _service.UpdateAsync(dto);
                            TempData["NotificationMessage"] = $"Swal.fire('{t("Sla_Notification_Inactivated")}','{t("Sla_Notification_Inactivated")}','success')";
                        }
                        else if (estadoForm == "0")
                        {
                            dto.Activo = true;
                            await _service.UpdateAsync(dto);
                            TempData["NotificationMessage"] = $"Swal.fire('{t("Sla_Notification_Reactivated")}','{t("Sla_Notification_Reactivated")}','success')";
                        }
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["LastDbError"] = ex.Message;
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
