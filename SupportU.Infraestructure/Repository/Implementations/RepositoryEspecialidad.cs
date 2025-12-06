using Microsoft.EntityFrameworkCore;
using SupportU.Infraestructure.Data;
using SupportU.Infraestructure.Models;
namespace SupportU.Infrastructure.Repository
{
	public class RepositoryEspecialidad : IRepositoryEspecialidad
	{
		private readonly SupportUContext _context;
		public RepositoryEspecialidad(SupportUContext context) => _context = context;
		public async Task<List<Especialidad>> ListAsync()
		{
			return await _context.Especialidad.ToListAsync();
		}
		public async Task<Especialidad?> FindByIdAsync(int id)
		{
			return await _context.Especialidad.FindAsync(id);
		}
		public async Task<int> AddAsync(Especialidad entity)
		{
			await _context.Especialidad.AddAsync(entity);
			await _context.SaveChangesAsync();
			return entity.EspecialidadId;
		}
		public async Task UpdateAsync()
		{
			await _context.SaveChangesAsync();
		}
		public async Task DeleteAsync(int id)
		{
			var entity = await _context.Especialidad.FindAsync(id);
			if (entity == null) return;
			entity.Activa = false;
			await _context.SaveChangesAsync();
		}
	}
}