using SupportU.Application.DTOs;

namespace SupportU.Application.Services
{
    public interface IServiceSla
    {
        Task<List<SlaDTO>> ListAsync();
        Task<SlaDTO?> FindByIdAsync(int id);
        Task<int> AddAsync(SlaDTO dto);
        Task UpdateAsync(SlaDTO dto);
        Task DeleteAsync(int id);
    }
}
