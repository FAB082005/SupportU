using SupportU.Infraestructure.Models;

namespace SupportU.Infrastructure.Repository
{
    public interface IRepositorySla
    {
        Task<List<Sla>> ListAsync();
        Task<Sla?> FindByIdAsync(int id);
        Task<int> AddAsync(Sla entity);
        Task UpdateAsync();
        Task DeleteAsync(int id);
    }
}
