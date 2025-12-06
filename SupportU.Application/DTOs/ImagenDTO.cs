using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportU.Application.DTOs
{
	public class ImagenDTO
	{
		public int ImagenId { get; set; }
		public int? TicketId { get; set; }
		public int? HistorialEstadoId { get; set; } 
		public string NombreArchivo { get; set; } = null!;
		public string RutaArchivo { get; set; } = null!;
		public DateTime FechaCreacion { get; set; } = DateTime.Now;

		// Propiedades de navegación
		public virtual HistorialEstadosDTO? HistorialEstado { get; set; }
		public virtual TicketDTO? Ticket { get; set; }
	}
}
