using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SupportU.Application.DTOs;
using SupportU.Application.Services.Interfaces;
using SupportU.Infrastructure.Repository.Interfaces;

namespace SupportU.Web.Controllers
{
    public class LoginController : Controller
    {
        private readonly IServiceUsuario _serviceUsuario;
        private readonly IRepositoryUsuario _repoUsuario;
        private readonly IConfiguration _config;

        public LoginController(IServiceUsuario serviceUsuario, IRepositoryUsuario repoUsuario, IConfiguration config)
        {
            _serviceUsuario = serviceUsuario ?? throw new ArgumentNullException(nameof(serviceUsuario));
            _repoUsuario = repoUsuario ?? throw new ArgumentNullException(nameof(repoUsuario));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }


        // GET: /Login
        public IActionResult Index()
        {
            return View();
        }

        // POST: /Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Login model, string? returnUrl)
        {
            if (!ModelState.IsValid)
            {
                TempData["ToastMessage"] = "toastr.error('Corrige los datos del formulario','Error');";
                return View("Index", model);
            }

            var usuarios = await _serviceUsuario.ListAsync();
            var usuario = usuarios.FirstOrDefault(u =>
                string.Equals(u.Email?.Trim(), model.Email?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (usuario == null)
            {
                TempData["ToastMessage"] = "toastr.error('Correo no registrado','Error de acceso');";
                return View("Index", model);
            }
            if (!usuario.Activo)
            {
                TempData["ToastMessage"] = "toastr.warning('Usuario inactivo. Contacte al administrador','Acceso denegado');";
                return View("Index", model);
            }

            var stored = usuario.PasswordHash ?? string.Empty;
            var hasher = new PasswordHasher<object>();
            var result = hasher.VerifyHashedPassword(null, stored, model.Password ?? string.Empty);


            if (result == PasswordVerificationResult.Success)
            {
                await SignIn(usuario, model.RememberMe);
                TempData["ToastMessage"] = $"toastr.success('Bienvenido {usuario.Nombre}','Conectado');";

                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            TempData["ToastMessage"] = "toastr.error('Contraseña incorrecta','Error de acceso');";
            return View("Index", model);
        }


        private async Task SignIn(UsuarioDTO usuario, bool rememberMe)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
                new Claim(ClaimTypes.Name, usuario.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, usuario.Rol ?? "Cliente")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var props = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                AllowRefresh = true,
                ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddHours(8) : (DateTimeOffset?)null
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);
        }

        // POST: /Login/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }

        // GET: /Login/ForgotPassword
        public IActionResult ForgotPassword()
        {
            return View(new UsuarioDTO());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(UsuarioDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "El correo es obligatorio");
                return View(model);
            }
            if (string.IsNullOrWhiteSpace(model.PasswordHash))
            {
                ModelState.AddModelError(nameof(model.PasswordHash), "La nueva contraseña es obligatoria");
                return View(model);
            }

            var usuarios = await _serviceUsuario.ListAsync();
            var usuario = usuarios.FirstOrDefault(u =>
                string.Equals(u.Email?.Trim(), model.Email?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (usuario == null)
            {
                ModelState.AddModelError(nameof(model.Email), "Correo no registrado");
                return View(model);
            }

            usuario.PasswordHash = model.PasswordHash;

            await _serviceUsuario.UpdateAsync(usuario);

            TempData["ToastMessage"] = "toastr.success('Contraseña actualizada correctamente','Éxito');";
            return RedirectToAction("Index", "Login");
        }

    }
}