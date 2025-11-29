using SupportU.Application.DTOs;

namespace SupportU.Application.Services
{
    public interface IServiceCategoria
    {
        Task<List<CategoriaDTO>> ListAsync();
        Task<CategoriaDTO?> FindByIdAsync(int id);
        Task<int> AddAsync(CategoriaDTO dto);
        Task UpdateAsync(CategoriaDTO dto);
        Task DeleteAsync(int id);
    }
}
