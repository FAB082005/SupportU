using SupportU.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportU.Application.Services.Interfaces
{
	public interface IServiceNotificacion
	{
		Task<List<NotificacionDTO>> GetByUserIdAsync(int usuarioId);
		Task<int> GetPendingCountAsync(int usuarioId);
		Task<bool> MarkAsReadAsync(int notificacionId, int usuarioId);
		Task CreateNotificationAsync(int usuarioDestinatarioId, int? ticketId, string tipo, string mensaje);
	}
}
