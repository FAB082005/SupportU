using Microsoft.EntityFrameworkCore;
using SupportU.Infraestructure.Data;
using SupportU.Infraestructure.Models;

namespace SupportU.Infrastructure.Repository
{
    public class RepositorySla : IRepositorySla
    {
        private readonly SupportUContext _context;
        public RepositorySla(SupportUContext context) => _context = context;

        public async Task<List<Sla>> ListAsync()
        {
            return await _context.Sla.ToListAsync();
        }

        public async Task<Sla?> FindByIdAsync(int id)
        {
            return await _context.Sla.FindAsync(id);
        }

        public async Task<int> AddAsync(Sla entity)
        {
            await _context.Sla.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity.SlaId;
        }

        public async Task UpdateAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Sla.FindAsync(id);
            if (entity == null) return;
            entity.Activo = false;
            await _context.SaveChangesAsync();
        }
    }
}
