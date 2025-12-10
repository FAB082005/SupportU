using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportU.Infraestructure.Models
{
	public class CategoriaEspecialidad
	{
		public int CategoriaId { get; set; }
		public int EspecialidadId { get; set; }

		// Navegación
		public virtual Categoria? Categoria { get; set; }
		public virtual Especialidad? Especialidad { get; set; }
	}
}
