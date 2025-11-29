using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SupportU.Infraestructure.Data;
using SupportU.Infraestructure.Models;

namespace SupportU.Infrastructure.Repository
{
    public class RepositoryCategoria : IRepositoryCategoria
    {
        private readonly SupportUContext _context;
        private readonly ILogger<RepositoryCategoria> _logger;

        public RepositoryCategoria(SupportUContext context, ILogger<RepositoryCategoria> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Categoria>> ListAsync()
        {
            return await _context.Categoria
                .Include(c => c.Sla)
                .Include(c => c.Especialidad)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Categoria?> FindByIdAsync(int id)
        {
            return await _context.Categoria
                .Include(c => c.Sla)
                .Include(c => c.Especialidad)
                .Include(c => c.Etiqueta)
                .FirstOrDefaultAsync(c => c.CategoriaId == id);
        }

        public async Task<int> AddAsync(Categoria entity)
        {
            _logger.LogInformation("RepositoryCategoria.AddAsync called. Nombre={Nombre}", entity?.Nombre);
            await _context.Categoria.AddAsync(entity);
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("RepositoryCategoria.AddAsync completed. Id={Id}", entity.CategoriaId);
                return entity.CategoriaId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RepositoryCategoria.AddAsync SaveChanges failed for Nombre={Nombre}", entity?.Nombre);
                throw;
            }
        }

        public async Task UpdateAsync(Categoria entity)
        {
            _logger.LogInformation("RepositoryCategoria.UpdateAsync called. Id={Id}", entity?.CategoriaId);
            _context.Categoria.Update(entity);
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("RepositoryCategoria.UpdateAsync completed. Id={Id}", entity.CategoriaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RepositoryCategoria.UpdateAsync SaveChanges failed. Id={Id}", entity?.CategoriaId);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Categoria.FindAsync(id);
            if (entity == null) return;
            entity.Activa = false;
            await _context.SaveChangesAsync();
        }
    }
}