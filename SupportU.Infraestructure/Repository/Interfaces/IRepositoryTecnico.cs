using SupportU.Infraestructure.Models;

namespace SupportU.Infrastructure.Repository
{
    public interface IRepositoryTecnico
    {
        Task<List<Tecnico>> ListAsync();
        Task<Tecnico?> FindByUsuarioIdAsync(int usuarioId);

        Task<int> AddAsync(Tecnico entity);
		Task UpdateAsync(Tecnico entity);
		Task DeleteByUsuarioIdAsync(int usuarioId);
		Task<Tecnico?> FindByIdAsync(int tecnicoId);
	}
}
