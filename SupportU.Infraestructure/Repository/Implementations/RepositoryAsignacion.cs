using Microsoft.EntityFrameworkCore;
using SupportU.Infraestructure.Data;
using SupportU.Infraestructure.Models;
using SupportU.Infraestructure.Repository.Interfaces;

namespace SupportU.Infraestructure.Repository.Implementations
{
    public class RepositoryAsignacion : IRepositoryAsignacion
    {
        private readonly SupportUContext _context;

        public RepositoryAsignacion(SupportUContext context)
        {
            _context = context;
        }

        public async Task<ICollection<Asignacion>> ListAsync()
        {
            
            var collection = await _context.Set<Asignacion>()
                .Include(a => a.Ticket)
                    .ThenInclude(t => t.Categoria)
                        .ThenInclude(c => c.Sla)
                .Include(a => a.Ticket)
                    .ThenInclude(t => t.Categoria)
                        .ThenInclude(c => c.Etiqueta)
                .Include(a => a.Ticket)
                    .ThenInclude(t => t.UsuarioSolicitante)
                .Include(a => a.Ticket)
                    .ThenInclude(t => t.TecnicoAsignado)
                        .ThenInclude(ta => ta.Usuario)
                .Include(a => a.Tecnico)
                    .ThenInclude(t => t.Usuario)
                .Include(a => a.UsuarioAsignador)
                .AsNoTracking()
                .OrderBy(a => a.FechaAsignacion)
                .ToListAsync();

            return collection;
        }

        public async Task<Asignacion> FindByIdAsync(int id)
        {
            var @object = await _context.Set<Asignacion>()
                .Include(a => a.Ticket)
                    .ThenInclude(t => t.Categoria)
                        .ThenInclude(c => c.Sla)
                .Include(a => a.Ticket)
                    .ThenInclude(t => t.Categoria)
                        .ThenInclude(c => c.Etiqueta)
                .Include(a => a.Ticket)
                    .ThenInclude(t => t.UsuarioSolicitante)
                .Include(a => a.Ticket)
                    .ThenInclude(t => t.TecnicoAsignado)
                        .ThenInclude(ta => ta.Usuario)
                .Include(a => a.Tecnico)
                    .ThenInclude(t => t.Usuario)
                .Include(a => a.UsuarioAsignador)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AsignacionId == id);

            return @object!;
        }

        public async Task<ICollection<Asignacion>> ListByTecnicoSemanaAsync(
            int tecnicoId,
            DateTime inicioSemana,
            DateTime finSemana)
        {
            var inicio = inicioSemana.Date;
            var fin = finSemana.Date.AddDays(1).AddTicks(-1);

            var collection = await _context.Set<Asignacion>()
                .Include(a => a.Ticket)
                    .ThenInclude(t => t.Categoria)
                        .ThenInclude(c => c.Sla)
                .Include(a => a.Ticket)
                    .ThenInclude(t => t.Categoria)
                        .ThenInclude(c => c.Etiqueta)
                .Include(a => a.Ticket)
                    .ThenInclude(t => t.UsuarioSolicitante)
                .Include(a => a.Ticket)
                    .ThenInclude(t => t.TecnicoAsignado)
                        .ThenInclude(ta => ta.Usuario)
                .Include(a => a.Tecnico)
                    .ThenInclude(t => t.Usuario)
                .Include(a => a.UsuarioAsignador)
                .Where(a => a.TecnicoId == tecnicoId
                    && a.FechaAsignacion >= inicio
                    && a.FechaAsignacion <= fin)
                .OrderBy(a => a.FechaAsignacion)
                .AsNoTracking()
                .ToListAsync();

            return collection;
        }
    }
}