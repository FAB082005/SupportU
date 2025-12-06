using System;
using System.Collections.Generic;

namespace SupportU.Application.DTOs
{
	public class TecnicoDTO
	{
		public int TecnicoId { get; set; }
		public int UsuarioId { get; set; }
		public int CargaTrabajo { get; set; }
		public string Estado { get; set; } = "Disponible";
		public decimal CalificacionPromedio { get; set; }


		public virtual UsuarioDTO? Usuario { get; set; }

		
		public virtual List<EspecialidadDTO>? Especialidades { get; set; }

		
		public string NombreUsuario => Usuario?.Nombre ?? "Sin nombre";
		public string CorreoUsuario => Usuario?.Email ?? "";
	}
}