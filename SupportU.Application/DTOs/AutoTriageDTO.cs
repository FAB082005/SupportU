using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportU.Application.DTOs
{
	public class AutoTriageDTO
	{
		public int TicketId { get; set; }
		public int TecnicoId { get; set; }
		public string NombreTecnico { get; set; } = string.Empty;
		public int Puntaje { get; set; }
		public string Justificacion { get; set; } = string.Empty;
		public bool Exitoso { get; set; }
		public string? MensajeError { get; set; }
	}
}
