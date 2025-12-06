// Application/Services/Implementations/ServiceImagen.cs
using AutoMapper;
using SupportU.Application.DTOs;
using SupportU.Application.Services.Interfaces;
using SupportU.Infraestructure.Models;
using SupportU.Infraestructure.Repository.Interfaces;

namespace SupportU.Application.Services.Implementations
{
	public class ServiceImagen : IServiceImagen
	{
		private readonly IRepositoryImagen _repo;
		private readonly IMapper _mapper;

		public ServiceImagen(IRepositoryImagen repo, IMapper mapper)
		{
			_repo = repo;
			_mapper = mapper;
		}

		public async Task<int> AddAsync(ImagenDTO dto)
		{
			var entity = _mapper.Map<Imagen>(dto);
			return await _repo.AddAsync(entity);
		}

		public async Task<ImagenDTO?> FindByIdAsync(int id)
		{
			var entity = await _repo.FindByIdAsync(id);
			return entity == null ? null : _mapper.Map<ImagenDTO>(entity);
		}

		public async Task<ICollection<ImagenDTO>> GetByTicketIdAsync(int ticketId)
		{
			var entities = await _repo.GetByTicketIdAsync(ticketId);
			return _mapper.Map<ICollection<ImagenDTO>>(entities);
		}

		public async Task<ICollection<ImagenDTO>> GetByHistorialIdAsync(int historialId)
		{
			var entities = await _repo.GetByHistorialIdAsync(historialId);
			return _mapper.Map<ICollection<ImagenDTO>>(entities);
		}

		public async Task UpdateAsync(ImagenDTO dto)
		{
			var entity = await _repo.FindByIdAsync(dto.ImagenId);
			if (entity == null) throw new KeyNotFoundException("Imagen no encontrada");

			_mapper.Map(dto, entity);
			await _repo.UpdateAsync(entity);
		}

		public async Task DeleteAsync(int id)
		{
			await _repo.DeleteAsync(id);
		}
	}
}