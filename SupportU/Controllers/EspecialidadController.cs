using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SupportU.Application.DTOs;
using SupportU.Application.Services;

namespace SupportU.Web.Controllers
{
    public class EspecialidadController : Controller
    {
        private readonly IServiceEspecialidad _service;

        public EspecialidadController(IServiceEspecialidad service)
        {
            _service = service ?? throw new System.ArgumentNullException(nameof(service));
        }

        // GET: /Especialidad
        public async Task<IActionResult> Index()
        {
            var list = await _service.ListAsync();
            return View(list);
        }

        // GET: /Especialidad/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var dto = await _service.FindByIdAsync(id);
            if (dto == null) return NotFound();
            return View(dto);
        }

        // GET: /Especialidad/Create
        public IActionResult Create()
        {
            return View(new EspecialidadDTO { Activa = true });
        }

        // POST: /Especialidad/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EspecialidadDTO dto)
        {
            if (!ModelState.IsValid) return View(dto);

            await _service.AddAsync(dto);
            TempData["NotificationMessage"] = "Swal.fire('Éxito','Especialidad creada correctamente','success');";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Especialidad/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var dto = await _service.FindByIdAsync(id);
            if (dto == null) return NotFound();
            return View(dto);
        }

        // POST: /Especialidad/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EspecialidadDTO dto)
        {
            if (!ModelState.IsValid) return View(dto);

            try
            {
                await _service.UpdateAsync(dto);
                TempData["NotificationMessage"] = "Swal.fire('Éxito','Especialidad actualizada correctamente','success');";
                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error: " + ex.Message);
                return View(dto);
            }
        }

        // GET: /Especialidad/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var dto = await _service.FindByIdAsync(id);
            if (dto == null) return NotFound();
            return View(dto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var estadoForm = Request.Form["estado"].FirstOrDefault();
            if (!string.IsNullOrEmpty(estadoForm))
            {
                if (estadoForm == "1") // inactivar
                {
                    var dto = await _service.FindByIdAsync(id);
                    if (dto != null)
                    {
                        dto.Activa = false;
                        await _service.UpdateAsync(dto);
                        TempData["NotificationMessage"] = "Swal.fire('Éxito','Especialidad inactivada correctamente','success');";
                    }
                }
                else if (estadoForm == "0") // reactivar
                {
                    var dto = await _service.FindByIdAsync(id);
                    if (dto != null)
                    {
                        dto.Activa = true;
                        await _service.UpdateAsync(dto);
                        TempData["NotificationMessage"] = "Swal.fire('Éxito','Especialidad reactivada correctamente','success');";
                    }
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
