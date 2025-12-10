using SupportU.Infraestructure.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SupportU.Infraestructure.Repository.Interfaces
{
	public interface IRepositoryEtiqueta
	{
		Task<List<Etiqueta>> ListAsync();
		Task<Etiqueta?> GetByIdAsync(int id);
		Task<Etiqueta> CreateAsync(Etiqueta etiqueta);
		Task<Etiqueta?> UpdateAsync(Etiqueta etiqueta);
		Task<bool> DeleteAsync(int id);
		Task<List<Etiqueta>> GetByCategoriaIdAsync(int categoriaId);
		Task<bool> NombreExistsAsync(string nombre, int? excludeId = null);
	}
}
