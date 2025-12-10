using SupportU.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportU.Application.Services.Interfaces
{
	public interface IServiceValoracion
	{
		Task<List<ValoracionDTO>> ListAsync();
		Task<ValoracionDTO?> GetByIdAsync(int id);
		Task<ValoracionDTO?> GetByTicketIdAsync(int ticketId);
		Task<ValoracionDTO> CreateAsync(ValoracionDTO dto);
		Task<bool> CanCreateValoracionAsync(int ticketId);
		Task<List<ValoracionDTO>> GetByUsuarioIdAsync(int usuarioId);
	}
}
