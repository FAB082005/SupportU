using Microsoft.AspNetCore.Mvc;
using SupportU.Application.Services.Interfaces;
using SupportU.Application.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics;

namespace SupportU.Web.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly IServiceUsuario _service;
        private static readonly List<string> _roles = new() { "Administrador", "Técnico", "Cliente" };

        public UsuarioController(IServiceUsuario service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        // GET: /Usuario
        public async Task<IActionResult> Index()
        {
            var usuarios = await _service.ListAsync();
            ViewBag.NotificationMessage = TempData["NotificationMessage"];
            return View(usuarios);
        }

        // GET: /Usuario/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var usuario = await _service.FindByIdAsync(id);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        // GET: /Usuario/Create
        public IActionResult Create()
        {
            ViewBag.Roles = new SelectList(_roles);
            return View(new UsuarioDTO { Activo = true, FechaCreacion = DateTime.UtcNow, Rol = "Cliente" });
        }

        // POST: /Usuario/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsuarioDTO dto)
        {
            // Forzar rol Cliente y activo true en servidor
            dto.Rol = "Cliente";
            dto.Activo = true;

            ViewBag.Roles = new SelectList(_roles, dto.Rol);

            if (string.IsNullOrWhiteSpace(dto?.Email))
                ModelState.AddModelError(nameof(dto.Email), "El correo es obligatorio");
            else if (!System.Text.RegularExpressions.Regex.IsMatch(dto.Email.Trim(), @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
                ModelState.AddModelError(nameof(dto.Email), "Formato de correo inválido");

            if (string.IsNullOrWhiteSpace(dto?.Nombre))
                ModelState.AddModelError(nameof(dto.Nombre), "El nombre es obligatorio");

            if (string.IsNullOrWhiteSpace(dto?.Apellidos))
                ModelState.AddModelError(nameof(dto.Apellidos), "Los apellidos son obligatorios");

            if (string.IsNullOrWhiteSpace(dto?.PasswordHash))
                ModelState.AddModelError(nameof(dto.PasswordHash), "La contraseña es obligatoria");

            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var hasher = new PasswordHasher<object>();
            dto.PasswordHash = hasher.HashPassword(null, dto.PasswordHash);

            var newId = await _service.AddAsync(dto);

            TempData["NotificationMessage"] = "Swal.fire('Éxito','Usuario creado correctamente','success');";

            // Redirigir al Login para que el usuario inicie sesión
            return RedirectToAction("Index", "Login");
        }

        // GET: /Usuario/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _service.FindByIdAsync(id);
            if (item == null) return NotFound();
            ViewBag.Roles = new SelectList(_roles, item.Rol);
            return View(item);
        }

        // POST: /Usuario/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UsuarioDTO dto)
        {
            ViewBag.Roles = new SelectList(_roles, dto.Rol);

            var changePwd = Request.Form["changePassword"].FirstOrDefault();

            if (string.IsNullOrEmpty(changePwd) || changePwd != "on")
            {
                // No cambiar contraseña / preserva hash existente
                dto.PasswordHash = null;
            }
            else
            {
                // Usuario marcó "Cambiar contraseña" / re-hashear
                if (!string.IsNullOrWhiteSpace(dto.PasswordHash))
                {
                    var hasher = new PasswordHasher<object>();
                    dto.PasswordHash = hasher.HashPassword(null, dto.PasswordHash);
                }
            }

            if (!ModelState.IsValid) return View(dto);

            if (!_roles.Contains(dto.Rol))
            {
                ModelState.AddModelError(nameof(dto.Rol), "Rol no válido");
                return View(dto);
            }

            var original = await _service.FindByIdAsync(dto.UsuarioId);
            if (original == null) return NotFound();
            dto.Activo = original.Activo;

            try
            {
                await _service.UpdateAsync(dto);
                TempData["NotificationMessage"] = "Swal.fire('Éxito','Usuario actualizado correctamente','success');";
                return RedirectToAction(nameof(Index));
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbex)
            {
                ModelState.AddModelError(string.Empty, "Error BD: " + (dbex.InnerException?.Message ?? dbex.Message));
                return View(dto);
            }
            catch (ArgumentException argEx)
            {
                ModelState.AddModelError(string.Empty, argEx.Message);
                return View(dto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error inesperado: " + ex.Message);
                return View(dto);
            }
        }

        // GET: /Usuario/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _service.FindByIdAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        // POST: /Usuario/DeleteConfirmed
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var estadoForm = Request.Form["estado"].FirstOrDefault();
            bool? desiredActivo = null;

            if (!string.IsNullOrEmpty(estadoForm))
            {
                if (estadoForm == "1") desiredActivo = false;
                else if (estadoForm == "0") desiredActivo = true;
            }

            var dto = await _service.FindByIdAsync(id);
            if (dto == null) return NotFound();

            dto.Activo = desiredActivo ?? false;

            await _service.UpdateAsync(dto);

            if (dto.Activo)
                TempData["NotificationMessage"] = "Swal.fire('Éxito','Usuario reactivado correctamente','success');";
            else
                TempData["NotificationMessage"] = "Swal.fire('Éxito','Usuario inactivado correctamente','success');";

            return RedirectToAction(nameof(Index));
        }
    }
}
