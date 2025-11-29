using SupportU.Application.DTOs;
using SupportU.Infraestructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportU.Application.Services.Interfaces
{
    public interface IServiceTicket
    {
        Task<ICollection<TicketDTO>> ListAsync();
        Task<TicketDTO> FindByIdAsync(int id);
        Task<int> AddAsync(TicketDTO dto);
        Task UpdateAsync(TicketDTO dto);
        Task DeleteAsync(int id);
        

    }
}
