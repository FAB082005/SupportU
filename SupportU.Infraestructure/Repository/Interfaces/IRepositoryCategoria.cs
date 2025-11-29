using SupportU.Infraestructure.Models;

namespace SupportU.Infrastructure.Repository
{
    public interface IRepositoryCategoria
    {
        Task<List<Categoria>> ListAsync();
        Task<Categoria?> FindByIdAsync(int id);
        Task<int> AddAsync(Categoria entity);
        Task UpdateAsync(Categoria entity);
        Task DeleteAsync(int id);
    }
}
