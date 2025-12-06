using Microsoft.EntityFrameworkCore;
using SupportU.Infraestructure.Data;
using SupportU.Infraestructure.Models;
using SupportU.Infraestructure.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportU.Infraestructure.Repository.Implementations
{
	public class RepositoryNotificacion : IRepositoryNotificacion
	{
		private readonly SupportUContext _context;

		public RepositoryNotificacion(SupportUContext context) => _context = context;

		public async Task<List<Notificacion>> ListByUserIdAsync(int usuarioId)
		{
			return await _context.Notificacion
				.Include(n => n.UsuarioDestinatario)
				.Include(n => n.Ticket)
				.Where(n => n.UsuarioDestinatarioId == usuarioId)
				.OrderByDescending(n => n.FechaCreacion)
				.ToListAsync();
		}

		public async Task<int> CountPendingByUserIdAsync(int usuarioId)
		{
			return await _context.Notificacion
				.Where(n => n.UsuarioDestinatarioId == usuarioId && n.Estado == "Pendiente")
				.CountAsync();
		}

		public async Task<Notificacion?> GetByIdAsync(int notificacionId)
		{
			return await _context.Notificacion
				.Include(n => n.UsuarioDestinatario)
				.Include(n => n.Ticket)
				.FirstOrDefaultAsync(n => n.NotificacionId == notificacionId);
		}

		public async Task<Notificacion> CreateAsync(Notificacion notificacion)
		{
			_context.Notificacion.Add(notificacion);
			await _context.SaveChangesAsync();
			return notificacion;
		}

		public async Task<bool> MarkAsReadAsync(int notificacionId, int usuarioId)
		{
			var notificacion = await _context.Notificacion
				.FirstOrDefaultAsync(n => n.NotificacionId == notificacionId
					&& n.UsuarioDestinatarioId == usuarioId);

			if (notificacion == null) return false;

			notificacion.Estado = "Leida";
			await _context.SaveChangesAsync();
			return true;
		}
	}
}
