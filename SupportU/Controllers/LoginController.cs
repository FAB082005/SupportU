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
    public class LoginController : BaseController
    {
        private readonly IServiceUsuario _serviceUsuario;
        private readonly IRepositoryUsuario _repoUsuario;
        private readonly IServiceNotificacion _serviceNotificacion;
        private readonly IConfiguration _config;

        public LoginController(
            IServiceUsuario serviceUsuario,
            IRepositoryUsuario repoUsuario,
            IServiceNotificacion serviceNotificacion,
            IConfiguration config)
        {
            _serviceUsuario = serviceUsuario;
            _repoUsuario = repoUsuario;
            _serviceNotificacion = serviceNotificacion;
            _config = config;
        }

        // Helper local para traducciones
        private string t(string key)
        {
            var translations = ViewData["Translations"] as Dictionary<string, string>
                ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            return translations.TryGetValue(key, out var val) ? val : key;
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
                TempData["ToastMessage"] = $"toastr.error('{t("Login_Error_InvalidForm")}','{t("Login_Error")}');";
                return View("Index", model);
            }

            var usuarios = await _serviceUsuario.ListAsync();
            var usuario = usuarios.FirstOrDefault(u =>
                string.Equals(u.Email?.Trim(), model.Email?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (usuario == null)
            {
                TempData["ToastMessage"] = $"toastr.error('{t("Login_Error_EmailNotRegistered")}','{t("Login_Error")}');";
                return View("Index", model);
            }
            if (!usuario.Activo)
            {
                TempData["ToastMessage"] = $"toastr.warning('{t("Login_Warning_UserInactive")}','{t("Login_AccessDenied")}');";
                return View("Index", model);
            }

            var stored = usuario.PasswordHash ?? string.Empty;
            var hasher = new PasswordHasher<object>();
            var result = hasher.VerifyHashedPassword(null, stored, model.Password ?? string.Empty);

            if (result == PasswordVerificationResult.Success)
            {
                await SignIn(usuario, model.RememberMe);

                try
                {
                    // Determina el idioma activo sin depender de ViewData
                    var culture = System.Globalization.CultureInfo.CurrentUICulture?.Name ?? "es-CR";

                    // Plantilla literal por cultura (sin claves ni diccionarios)
                    var tpl = culture.Equals("en-US", StringComparison.OrdinalIgnoreCase)
                        ? "You signed in on {0} at {1}"
                        : "Iniciaste sesión el {0} a las {1}";

                    var mensaje = string.Format(
                        tpl,
                        DateTime.Now.ToString("dd/MM/yyyy"),
                        DateTime.Now.ToString("HH:mm")
                    );

                    await _serviceNotificacion.CreateNotificationAsync(
                        usuarioDestinatarioId: usuario.UsuarioId,
                        ticketId: null,
                        tipo: "InicioSesion",
                        mensaje: mensaje
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al crear notificación de inicio de sesión: {ex.Message}");
                }

                TempData["ToastMessage"] = $"toastr.success('{string.Format(t("Login_Welcome"), usuario.Nombre)}','{t("Login_Connected")}');";

                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            TempData["ToastMessage"] = $"toastr.error('{t("Login_Error_WrongPassword")}','{t("Login_Error")}');";
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
                ModelState.AddModelError(nameof(model.Email), t("Forgot_EmailRequired"));
                return View(model);
            }
            if (string.IsNullOrWhiteSpace(model.PasswordHash))
            {
                ModelState.AddModelError(nameof(model.PasswordHash), t("Forgot_NewPasswordRequired"));
                return View(model);
            }

            var usuarios = await _serviceUsuario.ListAsync();
            var usuario = usuarios.FirstOrDefault(u =>
                string.Equals(u.Email?.Trim(), model.Email?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (usuario == null)
            {
                ModelState.AddModelError(nameof(model.Email), t("Forgot_EmailNotRegistered"));
                return View(model);
            }

            usuario.PasswordHash = model.PasswordHash;
            await _serviceUsuario.UpdateAsync(usuario);

            TempData["ToastMessage"] = $"toastr.success('{t("Forgot_Success_PasswordUpdated")}','{t("Forgot_Success")}');";
            return RedirectToAction("Index", "Login");
        }
    }
}
