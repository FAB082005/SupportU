using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SupportU.Infraestructure.Data;
using SupportU.Infraestructure.Models;
using SupportU.Infraestructure.Repository.Interfaces;

namespace SupportU.Infraestructure.Repository.Implementations
{
	public class RepositoryImagen : IRepositoryImagen
	{
		private readonly SupportUContext _context;
		private readonly ILogger<RepositoryImagen> _logger;

		public RepositoryImagen(SupportUContext context, ILogger<RepositoryImagen> logger)
		{
			_context = context;
			_logger = logger;
		}

		public async Task<ICollection<Imagen>> ListAsync()
		{
			return await _context.Imagen
				.Include(i => i.Ticket)
				.Include(i => i.HistorialEstado)
				.AsNoTracking()
				.ToListAsync();
		}

		public async Task<Imagen?> FindByIdAsync(int id)
		{
			return await _context.Imagen
				.Include(i => i.Ticket)
				.Include(i => i.HistorialEstado)
				.FirstOrDefaultAsync(i => i.ImagenId == id);
		}

		public async Task<int> AddAsync(Imagen entity)
		{
			try
			{
				_logger.LogInformation("=== RepositoryImagen.AddAsync ===");
				_logger.LogInformation("NombreArchivo: {Nombre}", entity.NombreArchivo);
				_logger.LogInformation("RutaArchivo: {Ruta}", entity.RutaArchivo);
				_logger.LogInformation("TicketId: {TicketId}", entity.TicketId);
				_logger.LogInformation("HistorialEstadoId: {HistorialId}", entity.HistorialEstadoId);

	
				if (entity.TicketId == null)
				{
					throw new InvalidOperationException("La imagen debe tener un TicketId asignado");
				}

			
				if (entity.HistorialEstadoId != null)
				{
					_logger.LogInformation(" Imagen asociada a historial {HistorialId}", entity.HistorialEstadoId);
				}
				else
				{
					_logger.LogInformation(" Imagen asociada solo al ticket (sin historial específico)");
				}

				await _context.Imagen.AddAsync(entity);
				await _context.SaveChangesAsync();

				_logger.LogInformation("Imagen guardada exitosamente con ID: {ImagenId}", entity.ImagenId);
				return entity.ImagenId;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, " ERROR en RepositoryImagen.AddAsync");
				_logger.LogError("InnerException: {InnerException}", ex.InnerException?.Message);
				throw;
			}
		}

		public async Task UpdateAsync(Imagen entity)
		{
			_logger.LogInformation("RepositoryImagen.UpdateAsync - ImagenId: {ImagenId}", entity.ImagenId);
			_context.Imagen.Update(entity);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteAsync(int id)
		{
			var imagen = await _context.Imagen.FindAsync(id);
			if (imagen != null)
			{
				_context.Imagen.Remove(imagen);
				await _context.SaveChangesAsync();
				_logger.LogInformation("RepositoryImagen.DeleteAsync - ImagenId: {ImagenId} eliminada", id);
			}
		}

		public async Task<ICollection<Imagen>> GetByTicketIdAsync(int ticketId)
		{
			return await _context.Imagen
				.Where(i => i.TicketId == ticketId)
				.Include(i => i.HistorialEstado)
				.AsNoTracking()
				.ToListAsync();
		}

		public async Task<ICollection<Imagen>> GetByHistorialIdAsync(int historialId)
		{
			return await _context.Imagen
				.Where(i => i.HistorialEstadoId == historialId)
				.AsNoTracking()
				.ToListAsync();
		}
	}
}