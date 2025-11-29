using Microsoft.EntityFrameworkCore;
using SupportU.Infraestructure.Data;
using SupportU.Infraestructure.Models;

namespace SupportU.Infrastructure.Repository
{
    public class RepositoryTecnico : IRepositoryTecnico
    {
        private readonly SupportUContext _context;

        public RepositoryTecnico(SupportUContext context)
        {
            _context = context;
        }

        public async Task<List<Tecnico>> ListAsync()
        {
            var query =
                from u in _context.Usuario
                where u.Rol == "Técnico"
                join t in _context.Tecnico on u.UsuarioId equals t.UsuarioId into tecnicoJoin
                from t in tecnicoJoin.DefaultIfEmpty()
                select new Tecnico
                {
                    TecnicoId = t != null ? t.TecnicoId : 0,
                    UsuarioId = u.UsuarioId,
                    CargaTrabajo = t != null ? t.CargaTrabajo : 0,
                    Estado = t != null ? t.Estado : (u.Activo ? "Disponible" : "Ausente"),
                    CalificacionPromedio = t != null ? t.CalificacionPromedio : 0.00m,
                    Usuario = u
                };

            return await query.ToListAsync();
        }

        public async Task<Tecnico?> FindByUsuarioIdAsync(int usuarioId)
        {
            return await _context.Tecnico
                .Include(t => t.Usuario)
                .FirstOrDefaultAsync(t => t.UsuarioId == usuarioId);
        }

        public async Task<int> AddAsync(Tecnico entity)
        {
            await _context.Tecnico.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity.TecnicoId;
        }

        public async Task UpdateAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task DeleteByUsuarioIdAsync(int usuarioId)
        {
            var t = await _context.Tecnico.FirstOrDefaultAsync(x => x.UsuarioId == usuarioId);
            if (t != null)
            {
                _context.Tecnico.Remove(t);
                await _context.SaveChangesAsync();
            }
        }
    }
}
