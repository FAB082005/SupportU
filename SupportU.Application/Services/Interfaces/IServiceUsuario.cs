using SupportU.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SupportU.Application.Services.Interfaces
{
    public interface IServiceUsuario
    {
        Task<ICollection<UsuarioDTO>> ListAsync();
        Task<UsuarioDTO?> FindByIdAsync(int id);
        Task<int> AddAsync(UsuarioDTO dto);
        Task UpdateAsync(UsuarioDTO dto);
        Task DeleteAsync(int id);
    }
}
