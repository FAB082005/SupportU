using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SupportU.Infraestructure.Data;
using SupportU.Infraestructure.Models;
using SupportU.Infraestructure.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupportU.Infraestructure.Repository.Implementations
{
    public class RepositoryHistorialEstados : IRepositoryHistorialEstados
    {
        private readonly SupportUContext _context;
        private readonly ILogger<RepositoryHistorialEstados> _logger;

        public RepositoryHistorialEstados(SupportUContext context, ILogger<RepositoryHistorialEstados> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ICollection<HistorialEstado>> ListAsync()
        {
            _logger.LogInformation("RepositoryHistorialEstados.ListAsync called");

            return await _context.HistorialEstado
                .Include(h => h.Ticket)
                .Include(h => h.Usuario)
                .Include(h => h.Imagenes)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<HistorialEstado?> FindByIdAsync(int id)
        {
            _logger.LogInformation("RepositoryHistorialEstados.FindByIdAsync called. Id={Id}", id);

            return await _context.HistorialEstado
                .Include(h => h.Ticket)
                .Include(h => h.Usuario)
                .Include(h => h.Imagenes)
                .FirstOrDefaultAsync(h => h.HistorialId == id);
        }

        public async Task<int> AddAsync(HistorialEstado entity)
        {
            _logger.LogInformation("RepositoryHistorialEstados.AddAsync called. TicketId={TicketId}, EstadoNuevo={EstadoNuevo}", entity?.TicketId, entity?.EstadoNuevo);

            await _context.HistorialEstado.AddAsync(entity);
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("RepositoryHistorialEstados.AddAsync completed. Id={Id}", entity.HistorialId);
                return entity.HistorialId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RepositoryHistorialEstados.AddAsync failed for TicketId={TicketId}", entity?.TicketId);
                throw;
            }
        }

        public async Task UpdateAsync(HistorialEstado entity)
        {
            _logger.LogInformation("RepositoryHistorialEstados.UpdateAsync called. Id={Id}", entity?.HistorialId);

            _context.HistorialEstado.Update(entity);
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("RepositoryHistorialEstados.UpdateAsync completed. Id={Id}", entity.HistorialId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RepositoryHistorialEstados.UpdateAsync failed. Id={Id}", entity?.HistorialId);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("RepositoryHistorialEstados.DeleteAsync called. Id={Id}", id);

            var entity = await _context.HistorialEstado.FindAsync(id);
            if (entity == null)
            {
                _logger.LogWarning("RepositoryHistorialEstados.DeleteAsync: entity not found. Id={Id}", id);
                return;
            }

            _context.HistorialEstado.Remove(entity);
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("RepositoryHistorialEstados.DeleteAsync completed. Id={Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RepositoryHistorialEstados.DeleteAsync failed. Id={Id}", id);
                throw;
            }
        }
    }
}
