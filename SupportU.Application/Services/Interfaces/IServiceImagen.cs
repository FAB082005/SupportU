using SupportU.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportU.Application.Services.Interfaces
{
	public interface IServiceImagen
	{
		Task<int> AddAsync(ImagenDTO dto);
		Task<ImagenDTO?> FindByIdAsync(int id);
		Task<ICollection<ImagenDTO>> GetByTicketIdAsync(int ticketId);
		Task<ICollection<ImagenDTO>> GetByHistorialIdAsync(int historialId);
		Task UpdateAsync(ImagenDTO dto);
		Task DeleteAsync(int id);
	}
}
