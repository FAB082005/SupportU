using SupportU.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SupportU.Application.Services.Interfaces
{
	public interface IServiceEtiqueta
	{
		Task<List<EtiquetaDTO>> ListAsync();
		Task<EtiquetaDTO?> GetByIdAsync(int id);
		Task<EtiquetaDTO> CreateAsync(EtiquetaDTO dto);
		Task<EtiquetaDTO?> UpdateAsync(int id, EtiquetaDTO dto);
		Task<bool> DeleteAsync(int id);
		Task<List<EtiquetaDTO>> GetByCategoriaIdAsync(int categoriaId);
	}
}