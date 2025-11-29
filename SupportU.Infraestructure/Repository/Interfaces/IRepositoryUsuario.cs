using SupportU.Infraestructure.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SupportU.Infrastructure.Repository.Interfaces
{
    public interface IRepositoryUsuario
    {
        Task<ICollection<Usuario>> ListAsync();
        Task<Usuario?> FindByIdAsync(int id);
        Task<int> AddAsync(Usuario entity);    
        Task UpdateAsync();
        Task DeleteAsync(int id);

    }
}
