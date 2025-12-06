using AutoMapper;
using SupportU.Application.DTOs;
using SupportU.Infrastructure.Repository;
namespace SupportU.Application.Services
{
	public class ServiceTecnico : IServiceTecnico
	{
		private readonly IRepositoryTecnico _repo;
		private readonly IMapper _mapper;
		public ServiceTecnico(IRepositoryTecnico repo, IMapper mapper)
		{
			_repo = repo;
			_mapper = mapper;
		}

		public async Task<List<TecnicoDTO>> ListAsync()
		{
			var list = await _repo.ListAsync();
			return _mapper.Map<List<TecnicoDTO>>(list);
		}

		public async Task<TecnicoDTO?> FindByIdAsync(int id)
		{
			var tecnico = await _repo.FindByIdAsync(id);
			if (tecnico == null) return null;
			return _mapper.Map<TecnicoDTO>(tecnico);
		}

		public async Task<TecnicoDTO?> FindByUsuarioIdAsync(int usuarioId)
		{
			var tecnicos = await _repo.ListAsync();
			var tecnico = tecnicos.FirstOrDefault(t => t.UsuarioId == usuarioId);
			return _mapper.Map<TecnicoDTO>(tecnico);
		}

		public async Task IncrementarCargaAsync(int tecnicoId)
		{
			var tecnico = await _repo.FindByIdAsync(tecnicoId);
			if (tecnico != null)
			{
				tecnico.CargaTrabajo++;
				await _repo.UpdateAsync(tecnico);
			}
		}

		public async Task DecrementarCargaAsync(int tecnicoId)
		{
			var tecnico = await _repo.FindByIdAsync(tecnicoId);
			if (tecnico != null && tecnico.CargaTrabajo > 0)
			{
				tecnico.CargaTrabajo--;
				await _repo.UpdateAsync(tecnico);
			}
		}

		public async Task ActualizarEstadoAsync(int tecnicoId, string nuevoEstado)
		{
			var tecnico = await _repo.FindByIdAsync(tecnicoId);
			if (tecnico != null)
			{
				tecnico.Estado = nuevoEstado;
				await _repo.UpdateAsync(tecnico);
			}
		}
	}
}