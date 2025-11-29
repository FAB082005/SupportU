using SupportU.Infraestructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportU.Infraestructure.Repository.Interfaces
{
    public interface IRepositoryHistorialEstados
    {
        Task<ICollection<HistorialEstado>> ListAsync();
        Task<HistorialEstado> FindByIdAsync(int id);
        Task<int> AddAsync(HistorialEstado entity);
        Task UpdateAsync(HistorialEstado entity);
    }
}
