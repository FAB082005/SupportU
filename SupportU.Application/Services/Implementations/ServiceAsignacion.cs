using AutoMapper;
using SupportU.Application.DTOs;
using SupportU.Application.Services.Interfaces;
using SupportU.Infraestructure.Models;
using SupportU.Infraestructure.Repository.Interfaces;

namespace SupportU.Application.Services.Implementations
{
	public class ServiceAsignacion : IServiceAsignacion
	{
		private readonly IRepositoryAsignacion _repository;
		private readonly IMapper _mapper;

		public ServiceAsignacion(IRepositoryAsignacion repository, IMapper mapper)
		{
			_repository = repository;
			_mapper = mapper;
		}

		public async Task<AsignacionDTO> FindByIdAsync(int id)
		{
			var @object = await _repository.FindByIdAsync(id);
			var objectMapped = _mapper.Map<AsignacionDTO>(@object);
			return objectMapped;
		}

		public async Task<ICollection<AsignacionDTO>> ListAsync()
		{
			var list = await _repository.ListAsync();
			var collection = _mapper.Map<ICollection<AsignacionDTO>>(list);
			return collection;
		}

		public async Task<ICollection<AsignacionDTO>> ListByTecnicoSemanaAsync(int tecnicoId, DateTime inicioSemana, DateTime finSemana)
		{
			var list = await _repository.ListByTecnicoSemanaAsync(tecnicoId, inicioSemana, finSemana);
			var collection = _mapper.Map<ICollection<AsignacionDTO>>(list);
			return collection;
		}

		public async Task<int> AddAsync(AsignacionDTO dto)
		{
			var entity = _mapper.Map<Asignacion>(dto);
			return await _repository.AddAsync(entity);
		}
	}
}