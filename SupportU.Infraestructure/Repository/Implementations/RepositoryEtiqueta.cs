using Microsoft.EntityFrameworkCore;
using SupportU.Infraestructure.Data;
using SupportU.Infraestructure.Models;
using SupportU.Infraestructure.Repository.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupportU.Infraestructure.Repository.Implementations
{
	public class RepositoryEtiqueta : IRepositoryEtiqueta
	{
		private readonly SupportUContext _context;

		public RepositoryEtiqueta(SupportUContext context)
		{
			_context = context;
		}

		public async Task<List<Etiqueta>> ListAsync()
		{
			return await _context.Etiqueta
				.Include(e => e.Categoria)
				.OrderBy(e => e.Nombre)
				.ToListAsync();
		}

		public async Task<Etiqueta?> GetByIdAsync(int id)
		{
			return await _context.Etiqueta
				.Include(e => e.Categoria)
				.FirstOrDefaultAsync(e => e.EtiquetaId == id);
		}

		public async Task<Etiqueta> CreateAsync(Etiqueta etiqueta)
		{
			_context.Etiqueta.Add(etiqueta);
			await _context.SaveChangesAsync();

			await _context.Entry(etiqueta)
				.Reference(e => e.Categoria)
				.LoadAsync();

			return etiqueta;
		}

		public async Task<Etiqueta?> UpdateAsync(Etiqueta etiqueta)
		{
			var existing = await _context.Etiqueta.FindAsync(etiqueta.EtiquetaId);
			if (existing == null) return null;

			existing.Nombre = etiqueta.Nombre;
			existing.CategoriaId = etiqueta.CategoriaId;
			existing.Activa = etiqueta.Activa;

			await _context.SaveChangesAsync();

			await _context.Entry(existing)
				.Reference(e => e.Categoria)
				.LoadAsync();

			return existing;
		}

		public async Task<bool> DeleteAsync(int id)
		{
			var etiqueta = await _context.Etiqueta.FindAsync(id);
			if (etiqueta == null) return false;

			etiqueta.Activa = false;
			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<List<Etiqueta>> GetByCategoriaIdAsync(int categoriaId)
		{
			return await _context.Etiqueta
				.Include(e => e.Categoria)
				.Where(e => e.CategoriaId == categoriaId && e.Activa)
				.OrderBy(e => e.Nombre)
				.ToListAsync();
		}

		public async Task<bool> NombreExistsAsync(string nombre, int? excludeId = null)
		{
			var query = _context.Etiqueta.Where(e => e.Nombre.ToLower() == nombre.ToLower());

			if (excludeId.HasValue)
				query = query.Where(e => e.EtiquetaId != excludeId.Value);

			return await query.AnyAsync();
		}
	}
}