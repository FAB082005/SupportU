using SupportU.Infraestructure.Models;

namespace SupportU.Infrastructure.Repository
{
    public interface IRepositoryEspecialidad
    {
        Task<List<Especialidad>> ListAsync();
        Task<Especialidad?> FindByIdAsync(int id);
        Task<int> AddAsync(Especialidad entity);
        Task UpdateAsync();
        Task DeleteAsync(int id);
    }
}
