using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using SupportU.Application.DTOs;
using SupportU.Application.Services;
using SupportU.Application.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace SupportU.Web.Controllers
{
	public class EtiquetaController : BaseController
	{
		private readonly IServiceEtiqueta _service;
		private readonly IServiceCategoria _serviceCategoria;
		private readonly ILogger<EtiquetaController> _logger;

		public EtiquetaController(
			IServiceEtiqueta service,
			IServiceCategoria serviceCategoria,
			ILogger<EtiquetaController> logger)
		{
			_service = service;
			_serviceCategoria = serviceCategoria;
			_logger = logger;
		}

		private async Task CargarCategoriasAsync()
		{
			var categorias = await _serviceCategoria.ListAsync();
			ViewBag.Categorias = new SelectList(categorias, "CategoriaId", "Nombre");
		}

		// GET: Etiqueta
		public async Task<IActionResult> Index()
		{
			try
			{
				var list = await _service.ListAsync();
				return View(list);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al listar etiquetas");
				TempData["Error"] = "Error al cargar las etiquetas";
				return View();
			}
		}

		// GET: Etiqueta/Create
		public async Task<IActionResult> Create()
		{
			await CargarCategoriasAsync();
			return View(new EtiquetaDTO { Activa = true });
		}

		// POST: Etiqueta/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(EtiquetaDTO dto)
		{
			_logger.LogWarning($"DEBUG POST Create => Nombre: {dto.Nombre}, CategoriaId: {dto.CategoriaId}, Activa: {dto.Activa}");

			if (string.IsNullOrWhiteSpace(dto.Nombre))
			{
				ModelState.AddModelError("Nombre", "El nombre es requerido");
			}

			if (dto.CategoriaId <= 0)
			{
				ModelState.AddModelError("CategoriaId", "Debe seleccionar una categoría");
			}

			if (!ModelState.IsValid)
			{
				foreach (var kv in ModelState)
				{
					foreach (var err in kv.Value.Errors)
					{
						_logger.LogError($"DEBUG ModelStateError => Campo: {kv.Key}, Error: {err.ErrorMessage}");
					}
				}

				await CargarCategoriasAsync();
				return View(dto);
			}

			try
			{
				_logger.LogWarning("DEBUG => ModelState válido. Ejecutando CreateAsync");
				await _service.CreateAsync(dto);

				TempData["Success"] = "Etiqueta creada exitosamente";
				return RedirectToAction(nameof(Index));
			}
			catch (InvalidOperationException ex)
			{
				_logger.LogError(ex, $"DEBUG => InvalidOperation: {ex.Message}");
				ModelState.AddModelError("Nombre", ex.Message);
				await CargarCategoriasAsync();
				return View(dto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"DEBUG => Excepción en CreateAsync: {ex.Message}");
				ModelState.AddModelError("", "Error al crear la etiqueta");
				await CargarCategoriasAsync();
				return View(dto);
			}
		}

		// GET: Etiqueta/Edit/5
		public async Task<IActionResult> Edit(int id)
		{
			var dto = await _service.GetByIdAsync(id);
			if (dto == null) return NotFound();

			await CargarCategoriasAsync();
			return View(dto);
		}

		// POST: Etiqueta/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, EtiquetaDTO dto)
		{
			if (id != dto.EtiquetaId)
			{
				return BadRequest();
			}

			if (string.IsNullOrWhiteSpace(dto.Nombre))
			{
				ModelState.AddModelError("Nombre", "El nombre es requerido");
			}

			if (dto.CategoriaId <= 0)
			{
				ModelState.AddModelError("CategoriaId", "Debe seleccionar una categoría");
			}

			if (!ModelState.IsValid)
			{
				await CargarCategoriasAsync();
				return View(dto);
			}

			try
			{
				var updated = await _service.UpdateAsync(id, dto);
				if (updated == null) return NotFound();

				TempData["Success"] = "Etiqueta actualizada exitosamente";
				return RedirectToAction(nameof(Index));
			}
			catch (InvalidOperationException ex)
			{
				ModelState.AddModelError("Nombre", ex.Message);
				await CargarCategoriasAsync();
				return View(dto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al actualizar etiqueta");
				ModelState.AddModelError("", "Error al actualizar la etiqueta");
				await CargarCategoriasAsync();
				return View(dto);
			}
		}

		// GET: Etiqueta/Delete/5
		public async Task<IActionResult> Delete(int id)
		{
			var dto = await _service.GetByIdAsync(id);
			if (dto == null) return NotFound();
			return View(dto);
		}

		// POST: Etiqueta/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			try
			{
				await _service.DeleteAsync(id);
				TempData["Success"] = "Etiqueta desactivada exitosamente";
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al desactivar etiqueta");
				TempData["Error"] = "Error al desactivar la etiqueta";
				return RedirectToAction(nameof(Index));
			}
		}

		// AJAX: Get etiquetas by categoria
		[HttpGet]
		public async Task<IActionResult> GetByCategoria(int categoriaId)
		{
			try
			{
				var etiquetas = await _service.GetByCategoriaIdAsync(categoriaId);
				return Json(etiquetas);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al obtener etiquetas por categoría");
				return BadRequest(new { message = ex.Message });
			}
		}
	}
}