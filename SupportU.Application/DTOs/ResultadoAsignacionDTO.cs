using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportU.Application.DTOs
{
	public class ResultadoAsignacionDTO
	{
		public bool Exitoso { get; set; }
		public int? TicketId { get; set; }
		public int? TecnicoAsignadoId { get; set; }
		public string? TecnicoNombre { get; set; }
		public int? Puntaje { get; set; }
		public int? TiempoRestanteSLA { get; set; }
		public string? CriterioUsado { get; set; }
		public int? CargaTecnico { get; set; }
		public decimal? CalificacionTecnico { get; set; }
		public string? MensajeError { get; set; }
	}
}
