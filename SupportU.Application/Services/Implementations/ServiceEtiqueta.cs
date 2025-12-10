using AutoMapper;
using SupportU.Application.DTOs;
using SupportU.Application.Services.Interfaces;
using SupportU.Infraestructure.Models;
using SupportU.Infraestructure.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SupportU.Application.Services.Implementations
{
	public class ServiceEtiqueta : IServiceEtiqueta
	{
		private readonly IRepositoryEtiqueta _repo;
		private readonly IMapper _mapper;

		public ServiceEtiqueta(IRepositoryEtiqueta repo, IMapper mapper)
		{
			_repo = repo;
			_mapper = mapper;
		}

		public async Task<List<EtiquetaDTO>> ListAsync()
		{
			var list = await _repo.ListAsync();
			return _mapper.Map<List<EtiquetaDTO>>(list);
		}

		public async Task<EtiquetaDTO?> GetByIdAsync(int id)
		{
			var etiqueta = await _repo.GetByIdAsync(id);
			return etiqueta != null ? _mapper.Map<EtiquetaDTO>(etiqueta) : null;
		}

		public async Task<EtiquetaDTO> CreateAsync(EtiquetaDTO dto)
		{
			if (string.IsNullOrWhiteSpace(dto.Nombre))
				throw new ArgumentException("El nombre es requerido");

			if (dto.CategoriaId <= 0)
				throw new ArgumentException("Debe seleccionar una categoría");

			if (await _repo.NombreExistsAsync(dto.Nombre))
				throw new InvalidOperationException($"Ya existe una etiqueta con el nombre '{dto.Nombre}'");

			var etiqueta = _mapper.Map<Etiqueta>(dto);
			var created = await _repo.CreateAsync(etiqueta);
			return _mapper.Map<EtiquetaDTO>(created);
		}

		public async Task<EtiquetaDTO?> UpdateAsync(int id, EtiquetaDTO dto)
		{
			if (string.IsNullOrWhiteSpace(dto.Nombre))
				throw new ArgumentException("El nombre es requerido");

			if (dto.CategoriaId <= 0)
				throw new ArgumentException("Debe seleccionar una categoría");

			if (await _repo.NombreExistsAsync(dto.Nombre, id))
				throw new InvalidOperationException($"Ya existe otra etiqueta con el nombre '{dto.Nombre}'");

			var etiqueta = _mapper.Map<Etiqueta>(dto);
			etiqueta.EtiquetaId = id;

			var updated = await _repo.UpdateAsync(etiqueta);
			return updated != null ? _mapper.Map<EtiquetaDTO>(updated) : null;
		}

		public async Task<bool> DeleteAsync(int id)
		{
			return await _repo.DeleteAsync(id);
		}

		public async Task<List<EtiquetaDTO>> GetByCategoriaIdAsync(int categoriaId)
		{
			var list = await _repo.GetByCategoriaIdAsync(categoriaId);
			return _mapper.Map<List<EtiquetaDTO>>(list);
		}
	}
}