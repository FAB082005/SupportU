using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SupportU.Infraestructure.Data;
using SupportU.Infraestructure.Models;
using SupportU.Infraestructure.Repository.Interfaces;

namespace SupportU.Infraestructure.Repository.Implementations
{
	public class RepositoryTicket : IRepositoryTicket
	{
		private readonly SupportUContext _context;
		private readonly ILogger<RepositoryTicket> _logger;

		public RepositoryTicket(SupportUContext context, ILogger<RepositoryTicket> logger)
		{
			_context = context;
			_logger = logger;
		}

		public async Task<ICollection<Ticket>> ListAsync()
		{
			var collection = await _context.Set<Ticket>()
				.Include(t => t.Categoria)
					.ThenInclude(c => c.Sla)
				.Include(t => t.Categoria)
					.ThenInclude(c => c.Etiqueta)
				.Include(t => t.UsuarioSolicitante)
				.Include(t => t.TecnicoAsignado)
					.ThenInclude(ta => ta.Usuario)
				.Include(t => t.HistorialEstado)
					.ThenInclude(h => h.Usuario)
				.Include(t => t.HistorialEstado)
					.ThenInclude(h => h.Imagenes)
				.Include(t => t.Imagen)
				.Include(t => t.Valoracion)
				.AsNoTracking()
				.ToListAsync();

			return collection;
		}

		public async Task<Ticket> FindByIdAsync(int id)
		{
			var @object = await _context.Set<Ticket>()
				.Include(t => t.Categoria)
					.ThenInclude(c => c.Sla)
				.Include(t => t.Categoria)
					.ThenInclude(c => c.Etiqueta)
				.Include(t => t.UsuarioSolicitante)
				.Include(t => t.TecnicoAsignado)
					.ThenInclude(ta => ta.Usuario)
				.Include(t => t.HistorialEstado)
					.ThenInclude(h => h.Usuario)
				.Include(t => t.HistorialEstado)
					.ThenInclude(h => h.Imagenes)
				.Include(t => t.Imagen)
				.Include(t => t.Valoracion)
				.AsNoTracking()
				.FirstOrDefaultAsync(t => t.TicketId == id);

			return @object!;
		}

		public async Task<int> AddAsync(Ticket entity)
		{
			_logger.LogInformation("RepositoryTicket.AddAsync called. Titulo={Titulo}", entity?.Titulo);

			await _context.Ticket.AddAsync(entity);

			try
			{
				await _context.SaveChangesAsync();
				_logger.LogInformation("RepositoryTicket.AddAsync completed. Id={Id}", entity.TicketId);
				return entity.TicketId;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "RepositoryTicket.AddAsync SaveChanges failed for Titulo={Titulo}", entity?.Titulo);
				throw;
			}
		}

		public async Task UpdateAsync(Ticket entity)
		{
			_logger.LogInformation("RepositoryTicket.UpdateAsync called. Id={Id}", entity?.TicketId);

			_context.Ticket.Update(entity);

			try
			{
				await _context.SaveChangesAsync();
				_logger.LogInformation("RepositoryTicket.UpdateAsync completed. Id={Id}", entity.TicketId);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "RepositoryTicket.UpdateAsync SaveChanges failed. Id={Id}", entity?.TicketId);
				throw;
			}
		}

		public async Task<Ticket?> FindByIdAsyncNoTracking(int id)
		{
			return await _context.Ticket
		  .AsNoTracking()
		  .FirstOrDefaultAsync(t => t.TicketId == id);
		}

		public async Task<Ticket?> FindByIdAsyncForUpdate(int id)
		{
			_logger.LogInformation(" RepositoryTicket.FindByIdAsyncForUpdate - Buscando ticket {Id}", id);

			var ticket = await _context.Ticket
			
				.FirstOrDefaultAsync(t => t.TicketId == id);

			_logger.LogInformation(" Resultado: {Found}", ticket != null ? "ENCONTRADO" : "NULL");

			if (ticket != null)
			{
				_logger.LogInformation(" Ticket {Id}: Estado={Estado}, CategoriaId={Cat}, UsuarioId={User}",
					ticket.TicketId,
					ticket.Estado,
					ticket.CategoriaId,
					ticket.UsuarioSolicitanteId);
			}

			return ticket;
		}
	}
}