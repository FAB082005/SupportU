using SupportU.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportU.Application.Services.Interfaces
{
    public interface IServiceAsignacion
    {

        Task<ICollection<AsignacionDTO>> ListAsync();
        Task<AsignacionDTO> FindByIdAsync(int id);
        Task<ICollection<AsignacionDTO>> ListByTecnicoSemanaAsync(int tecnicoId, DateTime inicioSemana, DateTime finSemana);
		Task<int> AddAsync(AsignacionDTO dto);
	}
}
