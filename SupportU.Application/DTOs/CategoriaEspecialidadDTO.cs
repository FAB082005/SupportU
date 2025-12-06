using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportU.Application.DTOs
{
	public class CategoriaEspecialidadDTO
	{
		public int CategoriaId { get; set; }
		public int EspecialidadId { get; set; }

		// Navegación opcional
		public virtual EspecialidadDTO? Especialidad { get; set; }
	}
}
