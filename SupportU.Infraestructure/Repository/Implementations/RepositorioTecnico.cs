using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SupportU.Infraestructure.Data;
using SupportU.Infraestructure.Models;

namespace SupportU.Infrastructure.Repository
{
    public class RepositoryTecnico : IRepositoryTecnico
    {
        private readonly SupportUContext _context;
        private readonly ILogger<RepositoryTecnico> _logger;

        public RepositoryTecnico(SupportUContext context, ILogger<RepositoryTecnico> logger)
        {
            _context = context;
            _logger = logger;
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

            var tecnicos = await query.ToListAsync();

            // Cargar especialidades para técnicos existentes
            foreach (var tecnico in tecnicos.Where(t => t.TecnicoId > 0))
            {
                await _context.Entry(tecnico).Collection(t => t.Especialidad).LoadAsync();
            }

            return tecnicos;
        }

        public async Task<Tecnico?> FindByUsuarioIdAsync(int usuarioId)
        {
            return await _context.Tecnico
                .Include(t => t.Usuario)
                .Include(t => t.Especialidad)
                .FirstOrDefaultAsync(t => t.UsuarioId == usuarioId);
        }

        public async Task<Tecnico?> FindByIdAsync(int tecnicoId)
        {
            return await _context.Tecnico
                .Include(t => t.Usuario)
                .Include(t => t.Especialidad)
                .FirstOrDefaultAsync(t => t.TecnicoId == tecnicoId);
        }

        public async Task<int> AddAsync(Tecnico entity)
        {
            await _context.Tecnico.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity.TecnicoId;
        }

        public async Task UpdateAsync(Tecnico entity)
        {
            _context.Tecnico.Update(entity);
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

        public async Task UpdateEspecialidadesAsync(int tecnicoId, List<int> especialidadIds)
        {
            var tecnico = await _context.Tecnico
                .Include(t => t.Especialidad)
                .FirstOrDefaultAsync(t => t.TecnicoId == tecnicoId);

            if (tecnico == null) throw new KeyNotFoundException("Técnico no encontrado");

            var especialidades = await _context.Especialidad
                .Where(e => especialidadIds.Contains(e.EspecialidadId))
                .ToListAsync();

            tecnico.Especialidad.Clear();
            foreach (var esp in especialidades)
            {
                tecnico.Especialidad.Add(esp);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("RepositoryTecnico.UpdateEspecialidadesAsync updated tecnicoId={Id} count={Count}", tecnicoId, especialidades.Count);
        }
    }
}
