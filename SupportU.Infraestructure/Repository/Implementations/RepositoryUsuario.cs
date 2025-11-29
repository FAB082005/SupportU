using Microsoft.EntityFrameworkCore;
using SupportU.Infraestructure.Data;
using SupportU.Infraestructure.Models;
using SupportU.Infrastructure.Repository.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupportU.Infrastructure.Repository.Implementations
{
    public class RepositoryUsuario : IRepositoryUsuario
    {
        private readonly SupportUContext _context;

        public RepositoryUsuario(SupportUContext context)
        {
            _context = context;
        }

        public async Task<Usuario?> FindByIdAsync(int id)
        {
            return await _context.Usuario.FindAsync(id);
        }

        public async Task<ICollection<Usuario>> ListAsync()
        {
            return await _context.Usuario.ToListAsync();
        }

        public async Task<int> AddAsync(Usuario entity)
        {
            System.Diagnostics.Debug.WriteLine($"Repository.AddAsync START. entity.Email={entity?.Email}; entity.Nombre={entity?.Nombre}");
            await _context.Usuario.AddAsync(entity);
            try
            {
                await _context.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine($"Repository.AddAsync SaveChanges OK. UsuarioId={entity.UsuarioId}");
                return entity.UsuarioId;
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Repository.AddAsync SaveChanges FAILED. Exception: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Usuario.FindAsync(id);
            if (entity == null) return;
            entity.Activo = false;
            await _context.SaveChangesAsync();
        }
    }
}
