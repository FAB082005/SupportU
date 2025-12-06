using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace SupportU.Application.DTOs
{
	public class TecnicoEspecialidadDTO
	{
		public int TecnicoId { get; set; }
		public int EspecialidadId { get; set; }
		
		public virtual EspecialidadDTO? Especialidad { get; set; }
	}
}