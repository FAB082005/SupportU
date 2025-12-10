using SupportU.Infraestructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportU.Infraestructure.Repository.Interfaces
{
	public interface IRepositoryValoracion
	{

		Task<List<Valoracion>> ListAsync();
		Task<Valoracion?> GetByIdAsync(int id);
		Task<Valoracion?> GetByTicketIdAsync(int ticketId);
		Task<Valoracion> CreateAsync(Valoracion valoracion);
		Task<bool> ExistsForTicketAsync(int ticketId);
		Task<List<Valoracion>> GetByUsuarioIdAsync(int usuarioId);

	}
}
