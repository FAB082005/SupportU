using Microsoft.EntityFrameworkCore;
using SupportU.Infraestructure.Data;
using SupportU.Infraestructure.Models;
using SupportU.Infraestructure.Repository.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupportU.Infraestructure.Repository.Implementations
{
	public class RepositoryValoracion : IRepositoryValoracion
	{
		private readonly SupportUContext _context;

		public RepositoryValoracion(SupportUContext context)
		{
			_context = context;
		}

		public async Task<List<Valoracion>> ListAsync()
		{
			return await _context.Valoracion
				.Include(v => v.Ticket)
				.Include(v => v.Usuario)
				.OrderByDescending(v => v.FechaValoracion)
				.ToListAsync();
		}

		public async Task<Valoracion?> GetByIdAsync(int id)
		{
			return await _context.Valoracion
				.Include(v => v.Ticket)
				.Include(v => v.Usuario)
				.FirstOrDefaultAsync(v => v.ValoracionId == id);
		}

		public async Task<Valoracion?> GetByTicketIdAsync(int ticketId)
		{
			return await _context.Valoracion
				.Include(v => v.Ticket)
				.Include(v => v.Usuario)
				.FirstOrDefaultAsync(v => v.TicketId == ticketId);
		}

		public async Task<Valoracion> CreateAsync(Valoracion valoracion)
		{
			_context.Valoracion.Add(valoracion);
			await _context.SaveChangesAsync();

			// Recargar con las relaciones
			await _context.Entry(valoracion)
				.Reference(v => v.Ticket)
				.LoadAsync();

			await _context.Entry(valoracion)
				.Reference(v => v.Usuario)
				.LoadAsync();

			return valoracion;
		}

		public async Task<bool> ExistsForTicketAsync(int ticketId)
		{
			return await _context.Valoracion
				.AnyAsync(v => v.TicketId == ticketId);
		}

		public async Task<List<Valoracion>> GetByUsuarioIdAsync(int usuarioId)
		{
			return await _context.Valoracion
				.Include(v => v.Ticket)
				.Include(v => v.Usuario)
				.Where(v => v.UsuarioId == usuarioId)
				.OrderByDescending(v => v.FechaValoracion)
				.ToListAsync();
		}
	}
}