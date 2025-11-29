using SupportU.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SupportU.Application.Services
{
    public interface IServiceEspecialidad
    {
        Task<List<EspecialidadDTO>> ListAsync();
        Task<EspecialidadDTO?> FindByIdAsync(int id);
        Task<int> AddAsync(EspecialidadDTO dto);
        Task UpdateAsync(EspecialidadDTO dto);
        Task DeleteAsync(int id);
    }
}
