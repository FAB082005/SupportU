using SupportU.Infraestructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportU.Infraestructure.Repository.Interfaces
{
    public interface IRepositoryTicket
    {

        Task<ICollection<Ticket>> ListAsync();
        Task<Ticket> FindByIdAsync(int id);
        Task<int> AddAsync(Ticket entity);
        Task UpdateAsync(Ticket entity);
        Task<Ticket?> FindByIdAsyncNoTracking(int id);
        Task<Ticket?> FindByIdAsyncForUpdate(int id);
    }
}
