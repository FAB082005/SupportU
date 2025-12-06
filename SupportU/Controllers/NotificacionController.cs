using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportU.Application.Services.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SupportU.Web.Controllers
{
	[Authorize] 
	public class NotificacionesController : Controller
	{
		private readonly IServiceNotificacion _service;

		public NotificacionesController(IServiceNotificacion service)
		{
			_service = service;
		}

		// GET: /Notificaciones
		public async Task<IActionResult> Index()
		{
			// ✅ Obtener el ID del usuario autenticado usando Claims
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int usuarioId))
			{
				TempData["ToastMessage"] = "toastr.error('Usuario no autenticado','Error');";
				return RedirectToAction("Index", "Login");
			}

			var notificaciones = await _service.GetByUserIdAsync(usuarioId);
			return View(notificaciones);
		}

		// POST: /Notificaciones/MarcarLeida
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> MarcarLeida(int id)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int usuarioId))
			{
				return Json(new { success = false, message = "Usuario no autenticado" });
			}

			var result = await _service.MarkAsReadAsync(id, usuarioId);

			return Json(new { success = result });
		}

		// GET: /Notificaciones/GetCount (para el ícono)
		[HttpGet]
		public async Task<IActionResult> GetCount()
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int usuarioId))
			{
				return Json(0);
			}

			int count = await _service.GetPendingCountAsync(usuarioId);
			return Json(count);
		}
	}
}