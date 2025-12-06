using SupportU.Infraestructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportU.Infraestructure.Repository.Interfaces
{
	public interface IRepositoryImagen
	{
		Task<ICollection<Imagen>> ListAsync();
		Task<Imagen?> FindByIdAsync(int id);
		Task<int> AddAsync(Imagen entity);
		Task UpdateAsync(Imagen entity);
		Task DeleteAsync(int id);
		Task<ICollection<Imagen>> GetByTicketIdAsync(int ticketId);
		Task<ICollection<Imagen>> GetByHistorialIdAsync(int historialId);
	}
}
