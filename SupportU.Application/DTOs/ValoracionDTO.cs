using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportU.Application.DTOs
{
	public class ValoracionDTO
	{
		public int ValoracionId { get; set; }
		public int TicketId { get; set; }
		public int UsuarioId { get; set; }
		public int Puntaje { get; set; }
		public string? Comentario { get; set; }
		public DateTime FechaValoracion { get; set; }

		// Propiedades de navegación para mostrar en vistas
		public string? TicketTitulo { get; set; }
		public string? UsuarioNombre { get; set; }
	}
}
