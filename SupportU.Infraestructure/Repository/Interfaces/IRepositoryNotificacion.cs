using SupportU.Infraestructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportU.Infraestructure.Repository.Interfaces
{
	public interface IRepositoryNotificacion
	{
		Task<List<Notificacion>> ListByUserIdAsync(int usuarioId);
		Task<int> CountPendingByUserIdAsync(int usuarioId);
		Task<Notificacion?> GetByIdAsync(int notificacionId);
		Task<Notificacion> CreateAsync(Notificacion notificacion);
		Task<bool> MarkAsReadAsync(int notificacionId, int usuarioId);
	}
}
