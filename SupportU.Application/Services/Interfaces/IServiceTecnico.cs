using SupportU.Application.DTOs;

namespace SupportU.Application.Services
{
    public interface IServiceTecnico
    {
        Task<List<TecnicoDTO>> ListAsync();
		Task<TecnicoDTO?> FindByIdAsync(int id);
		Task<TecnicoDTO?> FindByUsuarioIdAsync(int usuarioId);
		Task IncrementarCargaAsync(int tecnicoId);
		Task DecrementarCargaAsync(int tecnicoId);
		Task ActualizarEstadoAsync(int tecnicoId, string nuevoEstado);
	}
}
