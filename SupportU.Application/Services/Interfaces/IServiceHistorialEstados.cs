using SupportU.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportU.Application.Services.Interfaces
{
    public interface IServiceHistorialEstados
    {
        Task<ICollection<HistorialEstadosDTO>> ListAsync();
        Task<HistorialEstadosDTO> FindByIdAsync(int id);
        Task<int> AddAsync(HistorialEstadosDTO dto);
        Task UpdateAsync(HistorialEstadosDTO dto);
        Task DeleteAsync(int id);
    }
}
