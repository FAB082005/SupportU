using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SupportU.Application.DTOs;
using SupportU.Application.Services;

namespace SupportU.Web.Controllers
{
    public class SlaController : Controller
    {
        private readonly IServiceSla _service;

        public SlaController(IServiceSla service)
        {
            _service = service ?? throw new System.ArgumentNullException(nameof(service));
        }

        // GET: /Sla
        public async Task<IActionResult> Index()
        {
            var list = await _service.ListAsync();
            return View(list);
        }

        // GET: /Sla/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var dto = await _service.FindByIdAsync(id);
            if (dto == null) return NotFound();
            return View(dto);
        }

        // GET: /Sla/Create
        public IActionResult Create()
        {
            return View(new SlaDTO { Activo = true });
        }

        // POST: /Sla/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SlaDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
            {
                ModelState.AddModelError(nameof(dto.Nombre), "Nombre es obligatorio");
            }
            if (dto.TiempoRespuestaMinutos <= 0)
            {
                ModelState.AddModelError(nameof(dto.TiempoRespuestaMinutos), "Tiempo de respuesta debe ser mayor a 0");
            }
            if (dto.TiempoResolucionMinutos <= 0)
            {
                ModelState.AddModelError(nameof(dto.TiempoResolucionMinutos), "Tiempo de resolución debe ser mayor a 0");
            }
            if (!ModelState.IsValid) return View(dto);

            try
            {
                await _service.AddAsync(dto);
                TempData["NotificationMessage"] = "Swal.fire('Éxito','SLA creado correctamente','success');";
                return RedirectToAction(nameof(Index));
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                var inner = dbEx.InnerException?.Message ?? dbEx.Message;
                ModelState.AddModelError(string.Empty, "Error BD: " + inner);
                TempData["LastDbError"] = inner;
                return View(dto);
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error inesperado: " + ex.Message);
                TempData["LastDbError"] = ex.Message;
                return View(dto);
            }
        }

        // GET: /Sla/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var dto = await _service.FindByIdAsync(id);
            if (dto == null) return NotFound();
            return View(dto);
        }

        // POST: /Sla/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SlaDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                ModelState.AddModelError(nameof(dto.Nombre), "Nombre es obligatorio");
            if (dto.TiempoRespuestaMinutos <= 0)
                ModelState.AddModelError(nameof(dto.TiempoRespuestaMinutos), "Tiempo de respuesta debe ser mayor a 0");
            if (dto.TiempoResolucionMinutos <= 0)
                ModelState.AddModelError(nameof(dto.TiempoResolucionMinutos), "Tiempo de resolución debe ser mayor a 0");

            if (!ModelState.IsValid) return View(dto);

            try
            {
                await _service.UpdateAsync(dto);
                TempData["NotificationMessage"] = "Swal.fire('Éxito','SLA actualizado correctamente','success');";
                return RedirectToAction(nameof(Index));
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                var inner = dbEx.InnerException?.Message ?? dbEx.Message;
                ModelState.AddModelError(string.Empty, "Error BD: " + inner);
                TempData["LastDbError"] = inner;
                return View(dto);
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error: " + ex.Message);
                TempData["LastDbError"] = ex.Message;
                return View(dto);
            }
        }

        // GET: /Sla/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var dto = await _service.FindByIdAsync(id);
            if (dto == null) return NotFound();
            return View(dto);
        }

        // POST: /Sla/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var estadoForm = Request.Form["estado"].FirstOrDefault();
            if (!string.IsNullOrEmpty(estadoForm))
            {
                var dto = await _service.FindByIdAsync(id);
                if (dto != null)
                {
                    if (estadoForm == "1") 
                    {
                        dto.Activo = false;
                        await _service.UpdateAsync(dto);
                        TempData["NotificationMessage"] = "Swal.fire('Éxito','SLA inactivado correctamente','success');";
                    }
                    else if (estadoForm == "0") 
                    {
                        dto.Activo = true;
                        await _service.UpdateAsync(dto);
                        TempData["NotificationMessage"] = "Swal.fire('Éxito','SLA reactivado correctamente','success');";
                    }
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
