using System;
using System.Collections.Generic;

namespace SupportU.Application.DTOs
{
	public class TecnicoDTO
	{
        public int TecnicoId { get; set; }
        public int UsuarioId { get; set; }
        public bool UsuarioActivo { get; set; }
        public int CargaTrabajo { get; set; }
        public string Estado { get; set; } = "Disponible";
        public decimal CalificacionPromedio { get; set; }

        public UsuarioDTO? Usuario { get; set; }
        public List<EspecialidadDTO>? Especialidades { get; set; }
        public List<int> EspecialidadIds { get; set; } = new List<int>();

        public string NombreUsuario => Usuario?.Nombre ?? "Sin nombre";
        public string CorreoUsuario => Usuario?.Email ?? "";

    }
}