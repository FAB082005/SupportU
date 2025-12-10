using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SupportU.Application.DTOs;
using SupportU.Infraestructure.Data;
using SupportU.Infraestructure.Models;
using SupportU.Infrastructure.Repository;

namespace SupportU.Application.Services
{
	public class ServiceCategoria : IServiceCategoria
	{
		private readonly IRepositoryCategoria _repo;
		private readonly SupportUContext _context;
		private readonly IMapper _mapper;
		private readonly ILogger<ServiceCategoria> _logger;

		public ServiceCategoria(
			IRepositoryCategoria repo,
			SupportUContext context,
			IMapper mapper,
			ILogger<ServiceCategoria> logger)
		{
			_repo = repo;
			_context = context;
			_mapper = mapper;
			_logger = logger;
		}

		public async Task<List<CategoriaDTO>> ListAsync()
		{
			var list = await _repo.ListAsync();
			var dtoList = _mapper.Map<List<CategoriaDTO>>(list);

			// Cargar las especialidades de cada categoría
			foreach (var dto in dtoList)
			{
				dto.EspecialidadesSeleccionadas = await ObtenerEspecialidadesPorCategoria(dto.CategoriaId);
			}

			return dtoList;
		}

		public async Task<CategoriaDTO?> FindByIdAsync(int id)
		{
			var entity = await _repo.FindByIdAsync(id);
			if (entity == null) return null;

			var dto = _mapper.Map<CategoriaDTO>(entity);
			dto.EspecialidadesSeleccionadas = await ObtenerEspecialidadesPorCategoria(id);

			return dto;
		}

		public async Task<int> AddAsync(CategoriaDTO dto)
		{
			var entity = new Categoria
			{
				Nombre = dto.Nombre?.Trim() ?? string.Empty,
				Descripcion = dto.Descripcion,
				SlaId = dto.SlaId,
				CriterioAsignacion = dto.CriterioAsignacion ?? string.Empty,
				Activa = dto.Activa
			};

			var id = await _repo.AddAsync(entity);
			_logger.LogInformation("ServiceCategoria.AddAsync created CategoriaId={Id}", id);

			// Asociar las especialidades seleccionadas usando EF
			if (dto.EspecialidadesSeleccionadas != null && dto.EspecialidadesSeleccionadas.Any())
			{
				await AsociarEspecialidades(id, dto.EspecialidadesSeleccionadas);
			}

			return id;
		}

		public async Task UpdateAsync(CategoriaDTO dto)
		{
			var entity = await _repo.FindByIdAsync(dto.CategoriaId);
			if (entity == null) throw new KeyNotFoundException("Categoría no encontrada");

			entity.Nombre = dto.Nombre?.Trim() ?? entity.Nombre;
			entity.Descripcion = dto.Descripcion;
			entity.SlaId = dto.SlaId;
			entity.CriterioAsignacion = dto.CriterioAsignacion ?? entity.CriterioAsignacion;
			entity.Activa = dto.Activa;

			await _repo.UpdateAsync(entity);
			_logger.LogInformation("ServiceCategoria.UpdateAsync updated CategoriaId={Id}", dto.CategoriaId);

			// Actualizar las especialidades usando EF
			await EliminarEspecialidadesCategoria(dto.CategoriaId);
			if (dto.EspecialidadesSeleccionadas != null && dto.EspecialidadesSeleccionadas.Any())
			{
				await AsociarEspecialidades(dto.CategoriaId, dto.EspecialidadesSeleccionadas);
			}
		}

		public async Task DeleteAsync(int id)
		{
			await _repo.DeleteAsync(id);
			_logger.LogInformation("ServiceCategoria.DeleteAsync deactivated CategoriaId={Id}", id);
		}

		// ✅ Métodos privados usando EF Context
		private async Task<List<int>> ObtenerEspecialidadesPorCategoria(int categoriaId)
		{
			return await _context.CategoriaEspecialidad
				.Where(ce => ce.CategoriaId == categoriaId)
				.Select(ce => ce.EspecialidadId)
				.ToListAsync();
		}

		private async Task AsociarEspecialidades(int categoriaId, List<int> especialidadIds)
		{
			foreach (var especialidadId in especialidadIds)
			{
				// Verificar si ya existe la relación
				var existe = await _context.CategoriaEspecialidad
					.AnyAsync(ce => ce.CategoriaId == categoriaId && ce.EspecialidadId == especialidadId);

				if (!existe)
				{
					var relacion = new CategoriaEspecialidad
					{
						CategoriaId = categoriaId,
						EspecialidadId = especialidadId
					};

					await _context.CategoriaEspecialidad.AddAsync(relacion);
					_logger.LogInformation("Asociada Especialidad {EspecialidadId} con Categoría {CategoriaId}",
						especialidadId, categoriaId);
				}
			}

			await _context.SaveChangesAsync();
		}

		private async Task EliminarEspecialidadesCategoria(int categoriaId)
		{
			var relaciones = await _context.CategoriaEspecialidad
				.Where(ce => ce.CategoriaId == categoriaId)
				.ToListAsync();

			_context.CategoriaEspecialidad.RemoveRange(relaciones);
			await _context.SaveChangesAsync();

			_logger.LogInformation("Eliminadas todas las especialidades de Categoría {CategoriaId}", categoriaId);
		}
	}
}