using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SupportU.Infraestructure.Data;
using SupportU.Infraestructure.Models;

namespace SupportU.Infrastructure.Repository
{
    public class RepositoryTecnico : IRepositoryTecnico
    {
        private readonly SupportUContext _context;
        private readonly Microsoft.Extensions.Logging.ILogger<RepositoryTecnico> _logger;

        public RepositoryTecnico(SupportUContext context, Microsoft.Extensions.Logging.ILogger<RepositoryTecnico> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Tecnico>> ListAsync()
        {
            // Sincronizar técnicos faltantes (usuarios con Rol = "Técnico" y Activo = true)
            var usuarioIds = await _context.Usuario
                .Where(u => u.Rol == "Técnico" && u.Activo)
                .Select(u => u.UsuarioId)
                .ToListAsync();

            if (usuarioIds.Any())
            {
                var existentes = await _context.Tecnico
                    .Where(t => usuarioIds.Contains(t.UsuarioId))
                    .Select(t => t.UsuarioId)
                    .ToListAsync();

                var faltantes = usuarioIds.Except(existentes).ToList();
                if (faltantes.Any())
                {
                    var nuevos = faltantes.Select(id => new Tecnico
                    {
                        UsuarioId = id,
                        CargaTrabajo = 0,
                        Estado = "Disponible",
                        CalificacionPromedio = 0.00m
                    }).ToList();

                    await _context.Tecnico.AddRangeAsync(nuevos);
                    await _context.SaveChangesAsync();
                }
            }

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

            foreach (var tecnico in tecnicos.Where(t => t.TecnicoId > 0))
            {
                var tracked = await _context.Tecnico
                    .Include(x => x.Especialidad)
                    .FirstOrDefaultAsync(x => x.TecnicoId == tecnico.TecnicoId);

                if (tracked != null)
                {
                    tecnico.Especialidad = tracked.Especialidad;
                }
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
        }
    }
}
