using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportU.Application.DTOs
{
	public class NotificacionDTO
	{
		public int NotificacionId { get; set; }
		public int UsuarioDestinatarioId { get; set; }
		public int? TicketId { get; set; }
		public string TipoNotificacion { get; set; } = null!;
		public string Mensaje { get; set; } = null!;
		public string Estado { get; set; } = null!;
		public DateTime FechaCreacion { get; set; }

		
		public string NombreDestinatario { get; set; } = string.Empty;
		public string TituloTicket { get; set; } = string.Empty;
		public bool EsPendiente => Estado == "Pendiente";
	}
}
